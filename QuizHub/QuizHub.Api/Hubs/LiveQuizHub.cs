using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Models;
using QuizHub.Services.DTOs.LiveRoom;
using QuizHub.Services.Interfaces;
using System.Collections.Concurrent;

namespace QuizHub.Api.Hubs
{
    [Authorize]
    public class LiveQuizHub : Hub
    {
        private readonly ILiveRoomService _liveRoomService;
        private static readonly ConcurrentDictionary<string, (string RoomCode, int UserId)> _connections = new();
        private static readonly ConcurrentDictionary<string, HashSet<int>> _roomUsers = new();
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _roomTimers = new();

        public LiveQuizHub(ILiveRoomService liveRoomService)
        {
            _liveRoomService = liveRoomService;
        }

        private int GetUserId() => int.Parse(Context.UserIdentifier!);

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var info))
            {
                if (_roomTimers.TryRemove(info.RoomCode, out var timer))
                {
                    timer.Cancel();
                    timer.Dispose();
                }

                if (_roomUsers.TryGetValue(info.RoomCode, out var users))
                {
                    if (users.Remove(info.UserId))
                        await Clients.Group(info.RoomCode).SendAsync("PlayerLeft", info.UserId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomCode)
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            _connections[Context.ConnectionId] = (RoomCode: roomCode, UserId: userId);

            var users = _roomUsers.GetOrAdd(roomCode, _ => new HashSet<int>());
            if (users.Add(userId))
                await Clients.Group(roomCode).SendAsync("PlayerJoined", userId);

            await Clients.Caller.SendAsync("RoomJoined", roomCode);

            // Load the room once
            var room = await _liveRoomService.GetRoomByCodeAsync(roomCode);
            var host = room.Players
                           .Where(p => p.LeftAt == null)
                           .OrderBy(p => p.JoinedAt)
                           .FirstOrDefault();

            var isCallerHost = host != null && host.UserId == userId;

            var lobbyForCaller = _liveRoomService.MapToLobbyDtoForCaller(room, userId, isCallerHost);
            var lobbyForOthers = _liveRoomService.MapToLobbyDtoForOthers(room);

            if (host == null)
            {
                Console.WriteLine($"🔴 CRITICAL: No host found in room {roomCode}");
                return;
            }

            var lobbyForHost = _liveRoomService.MapToLobbyDtoForCaller(room, host.UserId, true);

            // Send caller their lobby
            await Clients.Caller.SendAsync("LobbyStatus", lobbyForCaller);

            // Everyone except host gets the "others" view
            var otherConnections = _connections
                .Where(kvp => kvp.Value.RoomCode == roomCode && kvp.Value.UserId != host.UserId)
                .Select(kvp => kvp.Key)
                .ToList();

            await Clients.Clients(otherConnections).SendAsync("LobbyStatus", lobbyForOthers);

            // Send host their special host view
            var hostConnection = _connections.FirstOrDefault(kvp => kvp.Value.UserId == host.UserId && kvp.Value.RoomCode == roomCode).Key;
            if (hostConnection != null && hostConnection != Context.ConnectionId)
            {
                await Clients.Client(hostConnection).SendAsync("LobbyStatus", lobbyForHost);
            }
        }

        public async Task LeaveRoom(string roomCode)
        {
            var userId = GetUserId();

            if (!_roomUsers.TryGetValue(roomCode, out var users))
                return;

            if (!users.Remove(userId))
                return;

            await _liveRoomService.LeaveRoomAsync(roomCode, userId);

            await Clients.Caller.SendAsync("RoomLeft", roomCode);

            var room = await _liveRoomService.GetRoomByCodeAsync(roomCode);
            var host = room.Players
                           .Where(p => p.LeftAt == null)
                           .OrderBy(p => p.JoinedAt)
                           .FirstOrDefault();

            // Update all remaining players with new lobby status
            var remainingConnections = _connections
                .Where(kvp => kvp.Value.RoomCode == roomCode && kvp.Value.UserId != userId)
                .ToList();

            foreach (var kvp in remainingConnections)
            {
                var connId = kvp.Key;
                var uid = kvp.Value.UserId;
                var isHost = host != null && host.UserId == uid;

                var lobbyDto = _liveRoomService.MapToLobbyDtoForCaller(room, uid, isHost);
                await Clients.Client(connId).SendAsync("LobbyStatus", lobbyDto);
            }

            await Clients.Group(roomCode).SendAsync("PlayerLeft", userId);

            if (users.Count == 0)
                _roomUsers.TryRemove(roomCode, out _);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            _connections.TryRemove(Context.ConnectionId, out _);
        }

        public async Task SubmitAnswer(AnswerSubmissionDto submission)
        {
            var userId = GetUserId();
            if (!_connections.TryGetValue(Context.ConnectionId, out var info))
                throw new HubException("User not in a room");

            try
            {
                Console.WriteLine($"🟡 SubmitAnswer received - QuestionId: {submission.QuestionId}, Answer: {submission.Answer}, ClientTime: {submission.ClientSubmittedAt}");

                var success = await _liveRoomService.SubmitAnswerAsync(info.RoomCode, userId, submission);

                // CRITICAL: Always send AnswerSubmitted event, regardless of correctness
                await Clients.Caller.SendAsync("AnswerSubmitted", submission.QuestionId);

                var leaderboard = await _liveRoomService.GetLiveLeaderboardAsync(info.RoomCode);
                await Clients.Group(info.RoomCode).SendAsync("LeaderboardUpdated", leaderboard);

                // Check if all players have answered and end question early
                if (await _liveRoomService.HaveAllPlayersAnsweredAsync(info.RoomCode, submission.QuestionId))
                {
                    Console.WriteLine($"🟡 All players have answered question {submission.QuestionId}, ending question early");
                    await EndQuestion(info.RoomCode, submission.QuestionId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 SubmitAnswer error: {ex.Message}");
                Console.WriteLine($"🔴 Stack trace: {ex.StackTrace}");
                throw new HubException($"Failed to submit answer: {ex.Message}");
            }
        }

        public async Task StartQuiz(string roomCode)
        {
            var userId = GetUserId();
            var started = await _liveRoomService.StartRoomAsync(roomCode, userId);

            if (!started)
                throw new HubException("Only the host can start the quiz");

            await Clients.Group(roomCode).SendAsync("QuizStarted");

            await Task.Delay(1000);
            var initialLeaderboard = await _liveRoomService.GetLiveLeaderboardAsync(roomCode);
            await Clients.Group(roomCode).SendAsync("LeaderboardUpdated", initialLeaderboard);

            await Task.Delay(1000);
            await AdvanceQuestionInternal(roomCode);
        }

        public async Task AdvanceQuestion()
        {
            var userId = GetUserId();
            if (!_connections.TryGetValue(Context.ConnectionId, out var info))
                throw new HubException("You are not in a room");

            var lobby = await _liveRoomService.GetLobbyStatusAsync(info.RoomCode, userId);
            if (lobby.Players.First().UserId != userId)
                throw new HubException("Only the host can advance the question");

            await AdvanceQuestionInternal(info.RoomCode);
        }

        private async Task AdvanceQuestionInternal(string roomCode)
        {
            try
            {
                Console.WriteLine($"🟡 [AdvanceQuestionInternal] Starting for room {roomCode}");

                // Cancel any existing timer
                if (_roomTimers.TryRemove(roomCode, out var existingTimer))
                {
                    Console.WriteLine($"🟡 [AdvanceQuestionInternal] Cancelling existing timer for room {roomCode}");
                    existingTimer.Cancel();
                    existingTimer.Dispose();
                }

                var question = await _liveRoomService.GetCurrentQuestionAsync(roomCode);
                Console.WriteLine($"🟡 [AdvanceQuestionInternal] Got question: {(question != null ? $"ID {question.QuestionId}" : "NULL")}");

                if (question != null)
                {
                    Console.WriteLine($"🟡 [AdvanceQuestionInternal] Starting question {question.QuestionId} with {question.TimeRemaining}s timer");

                    await Clients.Group(roomCode).SendAsync("QuestionStarted", question);
                    Console.WriteLine($"🟡 [AdvanceQuestionInternal] QuestionStarted event sent");

                    // Create new cancellation token
                    var cts = new CancellationTokenSource();
                    _roomTimers[roomCode] = cts;

                    // Start timer task
                    _ = StartQuestionTimer(roomCode, question.QuestionId, question.TimeRemaining, cts.Token);
                }
                else
                {
                    Console.WriteLine($"🟡 [AdvanceQuestionInternal] No current question found, ending quiz");
                    var leaderboard = await _liveRoomService.GetLiveLeaderboardAsync(roomCode);
                    await Clients.Group(roomCode).SendAsync("QuizEnded", leaderboard);
                    _roomUsers.TryRemove(roomCode, out _);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [AdvanceQuestionInternal] Error: {ex.Message}");
                Console.WriteLine($"🔴 Stack trace: {ex.StackTrace}");
            }
        }

        private async Task StartQuestionTimer(string roomCode, int questionId, int timeRemaining, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"🟡 [Timer] Started for question {questionId}, waiting {timeRemaining}s");
                await Task.Delay(timeRemaining * 1000, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"🟡 [Timer] Cancelled for question {questionId}");
                    return;
                }

                Console.WriteLine($"🟡 [Timer] Expired for question {questionId}");
                await EndQuestion(roomCode, questionId);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"🟡 [Timer] Task cancelled for question {questionId}");
            }
        }

        private async Task EndQuestion(string roomCode, int questionId)
        {
            try
            {
                Console.WriteLine($"🟡 [1] EndQuestion called for room {roomCode}, question {questionId}");

                // Clean up timer
                if (_roomTimers.TryRemove(roomCode, out var timer))
                {
                    timer.Dispose();
                    Console.WriteLine($"🟡 [1.5] Timer cleaned up for room {roomCode}");
                }

                // Send QuestionEnded event
                await Clients.Group(roomCode).SendAsync("QuestionEnded", questionId);
                Console.WriteLine($"🟡 [2] QuestionEnded event sent for question {questionId}");

                // Get leaderboard update
                var leaderboard = await _liveRoomService.GetLiveLeaderboardAsync(roomCode);
                await Clients.Group(roomCode).SendAsync("LeaderboardUpdated", leaderboard);
                Console.WriteLine($"🟡 [3] LeaderboardUpdated sent");

                // Wait before advancing
                await Task.Delay(3000);
                Console.WriteLine($"🟡 [4] Delay completed, advancing question");

                // Advance to next question
                var hasMoreQuestions = await _liveRoomService.AdvanceQuestionAsync(roomCode);
                Console.WriteLine($"🟡 [5] After AdvanceQuestionAsync - hasMoreQuestions: {hasMoreQuestions}");

                if (hasMoreQuestions)
                {
                    Console.WriteLine($"🟡 [6] Advancing to next question in room {roomCode}");
                    await AdvanceQuestionInternal(roomCode);
                }
                else
                {
                    Console.WriteLine($"🟡 [6] No more questions, ending quiz for room {roomCode}");
                    var finalLeaderboard = await _liveRoomService.GetLiveLeaderboardAsync(roomCode);
                    await Clients.Group(roomCode).SendAsync("QuizEnded", finalLeaderboard);
                    _roomUsers.TryRemove(roomCode, out _);
                }

                Console.WriteLine($"🟡 [7] EndQuestion completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [EndQuestion] ERROR: {ex.Message}");
                Console.WriteLine($"🔴 [EndQuestion] Stack trace: {ex.StackTrace}");
            }
        }
    }
}