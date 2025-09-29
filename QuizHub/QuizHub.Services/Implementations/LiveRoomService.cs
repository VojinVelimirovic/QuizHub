using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizHub.Data.Context;
using QuizHub.Data.Models;
using QuizHub.Services.DTOs.LiveRoom;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.Interfaces;
using System.Text.Json;

namespace QuizHub.Services.Implementations
{
    public class LiveRoomService : ILiveRoomService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly Random _random;

        public LiveRoomService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _random = new Random();
        }

        public async Task<LiveRoomDto> CreateRoomAsync(LiveRoomCreateDto dto, int hostUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.Where(q => q.IsActive))
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId && q.IsActive);

            if (quiz == null)
                throw new InvalidOperationException("Quiz not found or is inactive");

            if (quiz.Questions.Count == 0)
                throw new InvalidOperationException("Quiz must have at least one question");

            if (dto.MaxPlayers < 2 || dto.MaxPlayers > 20)
                throw new InvalidOperationException("Max players must be between 2 and 20");

            if (dto.SecondsPerQuestion < 10 || dto.SecondsPerQuestion > 120)
                throw new InvalidOperationException("Seconds per question must be between 10 and 120");

            if (dto.StartDelaySeconds < 10 || dto.StartDelaySeconds > 300)
                throw new InvalidOperationException("Start delay must be between 10 and 300 seconds");

            var roomCode = GenerateUniqueRoomCode();

            var room = new LiveRoom
            {
                RoomCode = roomCode,
                Name = dto.Name,
                QuizId = dto.QuizId,
                MaxPlayers = dto.MaxPlayers,
                SecondsPerQuestion = dto.SecondsPerQuestion,
                StartDelaySeconds = dto.StartDelaySeconds,
                CreatedAt = DateTime.UtcNow,
                CurrentQuestionIndex = -1,
                IsActive = true
            };

            _context.LiveRooms.Add(room);
            await _context.SaveChangesAsync();

            return MapToLiveRoomDto(room);
        }

        public async Task<List<LiveRoomDto>> GetActiveRoomsAsync()
        {
            var rooms = await _context.LiveRooms
                .Include(lr => lr.Quiz)
                .Include(lr => lr.Players)
                .Where(lr => lr.IsActive &&
                            lr.StartedAt == null &&
                            lr.CreatedAt.AddSeconds(lr.StartDelaySeconds) > DateTime.UtcNow)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();

            return rooms.Select(MapToLiveRoomDto).ToList();
        }

        public async Task<LiveRoomLobbyDto> JoinRoomAsync(string roomCode, int userId)
        {
            var activePlayers = await _context.LiveRoomPlayers
                .Include(p => p.LiveRoom)
                .Where(p => p.UserId == userId && p.LeftAt == null && p.LiveRoom.RoomCode != roomCode)
                .ToListAsync();

            foreach (var player in activePlayers)
            {
                player.LeftAt = DateTime.UtcNow;
            }

            if (activePlayers.Any())
            {
                await _context.SaveChangesAsync();
                await Task.Delay(100);
            }

            var room = await _context.LiveRooms
                .Include(r => r.Players)
                    .ThenInclude(p => p.User)
                .Include(r => r.Quiz)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null)
                throw new InvalidOperationException("Room not found");
            if (room.StartedAt != null)
                throw new InvalidOperationException("Room has already started");
            if (room.EndedAt != null)
                throw new InvalidOperationException("Room has ended");

            var existingPlayer = room.Players.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);
            if (existingPlayer != null)
                return MapToLobbyDto(room, userId);

            var activePlayersCount = room.Players.Count(p => p.LeftAt == null);
            if (activePlayersCount >= room.MaxPlayers)
                throw new InvalidOperationException("Room is full");

            var maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var player = new LiveRoomPlayer
                    {
                        LiveRoomId = room.Id,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow,
                        Score = 0
                    };

                    _context.LiveRoomPlayers.Add(player);
                    await _context.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateException ex)
                {
                    if (i == maxRetries - 1) throw;

                    await Task.Delay(100 * (i + 1));

                    room = await _context.LiveRooms
                        .Include(r => r.Players)
                            .ThenInclude(p => p.User)
                        .Include(r => r.Quiz)
                        .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

                    existingPlayer = room.Players.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);
                    if (existingPlayer != null)
                        return MapToLobbyDto(room, userId);
                }
            }

            room = await _context.LiveRooms
                .Include(r => r.Players)
                    .ThenInclude(p => p.User)
                .Include(r => r.Quiz)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            return MapToLobbyDto(room, userId);
        }

        public async Task<bool> LeaveRoomAsync(string roomCode, int userId)
        {
            var room = await GetRoomByCodeAsync(roomCode);
            var player = room.Players.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);

            if (player != null)
            {
                player.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<LiveRoomLobbyDto> GetLobbyStatusAsync(string roomCode, int userId)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            if (!room.Players.Any(p => p.UserId == userId && p.LeftAt == null))
                throw new UnauthorizedAccessException("You are not in this room");

            return MapToLobbyDto(room, userId);
        }

        public async Task<bool> StartRoomAsync(string roomCode, int userId)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            var host = GetCurrentHost(room);
            if (host == null || host.UserId != userId)
                throw new UnauthorizedAccessException("Only the host can start the room");

            if (room.StartedAt != null)
                throw new InvalidOperationException("Room has already started");

            if (room.Players.Count(p => p.LeftAt == null) < 2)
                throw new InvalidOperationException("Need at least 2 players to start");

            room.StartedAt = DateTime.UtcNow;
            room.CurrentQuestionIndex = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LiveRoomQuestionDto> GetCurrentQuestionAsync(string roomCode)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            if (room.StartedAt == null)
                throw new InvalidOperationException("Room hasn't started yet");

            if (room.EndedAt != null)
                throw new InvalidOperationException("Room has ended");

            if (room.CurrentQuestionIndex < 0)
                throw new InvalidOperationException("No active question");

            var quiz = await _context.Quizzes
                .Include(q => q.Questions.Where(q => q.IsActive))
                    .ThenInclude(q => q.AnswerOptions.Where(ao => ao.IsActive))
                .FirstOrDefaultAsync(q => q.Id == room.QuizId);

            var questions = quiz.Questions
                .Where(q => q.IsActive)
                .OrderBy(q => q.Id)
                .ToList();

            if (room.CurrentQuestionIndex >= questions.Count)
            {
                throw new InvalidOperationException("Quiz has ended - no more questions");
            }

            var question = questions[room.CurrentQuestionIndex];
            var questionDto = _mapper.Map<LiveRoomQuestionDto>(question);

            questionDto.QuestionId = question.Id;
            questionDto.TimeRemaining = room.SecondsPerQuestion;
            questionDto.QuestionIndex = room.CurrentQuestionIndex + 1;
            questionDto.TotalQuestions = questions.Count;

            return questionDto;
        }

        public async Task<bool> SubmitAnswerAsync(string roomCode, int userId, AnswerSubmissionDto submission)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            if (room.EndedAt != null)
                throw new InvalidOperationException("Room has ended");

            if (room.CurrentQuestionIndex < 0)
                throw new InvalidOperationException("No active question");

            var existingAnswer = await _context.LiveRoomAnswers
                .FirstOrDefaultAsync(a => a.LiveRoomId == room.Id &&
                                        a.UserId == userId &&
                                        a.QuestionId == submission.QuestionId);

            if (existingAnswer != null)
                throw new InvalidOperationException("Already submitted answer for this question");

            var quiz = await _context.Quizzes
                .Include(q => q.Questions.Where(q => q.IsActive))
                    .ThenInclude(q => q.AnswerOptions.Where(ao => ao.IsActive))
                .FirstOrDefaultAsync(q => q.Id == room.QuizId);

            var questions = quiz.Questions.OrderBy(q => q.Id).ToList();

            if (room.CurrentQuestionIndex >= questions.Count)
                throw new InvalidOperationException("No active question");

            var currentQuestion = questions[room.CurrentQuestionIndex];

            if (currentQuestion.Id != submission.QuestionId)
            {
                throw new InvalidOperationException("This question is not currently active");
            }

            var questionStartTime = room.StartedAt.Value.AddSeconds(room.CurrentQuestionIndex * room.SecondsPerQuestion);

            var clientSubmitTime = DateTimeOffset.FromUnixTimeMilliseconds((long)submission.ClientSubmittedAt).UtcDateTime;
            var responseTime = (clientSubmitTime - questionStartTime).TotalSeconds;

            if (responseTime < 0) responseTime = 0;
            if (responseTime > room.SecondsPerQuestion) responseTime = room.SecondsPerQuestion;

            bool isCorrect = ValidateAnswer(currentQuestion, submission.Answer);
            bool gotFirstBlood = false;

            if (isCorrect)
            {
                var firstCorrectAnswer = await _context.LiveRoomAnswers
                    .Where(a => a.LiveRoomId == room.Id &&
                               a.QuestionId == submission.QuestionId &&
                               a.IsCorrect)
                    .OrderBy(a => a.SubmittedAt)
                    .FirstOrDefaultAsync();

                gotFirstBlood = firstCorrectAnswer == null;
            }

            var liveAnswer = new LiveRoomAnswer
            {
                LiveRoomId = room.Id,
                UserId = userId,
                QuestionId = submission.QuestionId,
                SubmittedAnswer = JsonSerializer.Serialize(submission.Answer),
                IsCorrect = isCorrect,
                SubmittedAt = clientSubmitTime,
                ResponseTimeSeconds = responseTime,
                GotFirstBlood = gotFirstBlood
            };

            _context.LiveRoomAnswers.Add(liveAnswer);

            var player = room.Players.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);
            if (player != null && isCorrect)
            {
                player.Score += 10;

                if (gotFirstBlood)
                {
                    player.Score += 5;
                }

                if (responseTime <= room.SecondsPerQuestion / 3.0)
                {
                    player.Score += 3;
                }
            }

            await _context.SaveChangesAsync();
            return isCorrect;
        }

        public async Task<LiveLeaderboardDto> GetLiveLeaderboardAsync(string roomCode)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            var players = room.Players
                .Where(p => p.LeftAt == null)
                .Select(p => new
                {
                    Player = p,
                    CorrectAnswers = room.Answers.Count(a => a.UserId == p.UserId && a.IsCorrect),
                    AverageResponseTime = room.Answers
                        .Where(a => a.UserId == p.UserId && a.IsCorrect)
                        .DefaultIfEmpty()
                        .Average(a => a == null ? 0 : a.ResponseTimeSeconds)
                })
                .OrderByDescending(x => x.Player.Score)
                .ThenBy(x => x.AverageResponseTime)
                .ToList();

            var leaderboard = new LiveLeaderboardDto
            {
                IsFinal = room.EndedAt != null,
                Entries = players.Select((x, index) => new LiveLeaderboardEntryDto
                {
                    Username = x.Player.User.Username,
                    Score = x.Player.Score,
                    CorrectAnswers = x.CorrectAnswers,
                    AverageResponseTime = x.AverageResponseTime,
                    Position = index + 1
                }).ToList()
            };

            return leaderboard;
        }

        public async Task<bool> AdvanceQuestionAsync(string roomCode)
        {
            try
            {
                var room = await _context.LiveRooms
                    .Include(r => r.Quiz)
                        .ThenInclude(q => q.Questions.Where(q => q.IsActive))
                    .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

                if (room == null)
                {
                    return false;
                }

                var activeQuestions = room.Quiz.Questions
                    .Where(q => q.IsActive)
                    .OrderBy(q => q.Id)
                    .ToList();

                var totalActiveQuestions = activeQuestions.Count;

                room.CurrentQuestionIndex++;

                if (room.CurrentQuestionIndex < totalActiveQuestions)
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    room.CurrentQuestionIndex = -1;
                    room.EndedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> EndRoomAsync(string roomCode)
        {
            var room = await GetRoomByCodeAsync(roomCode);

            if (room.EndedAt == null)
            {
                room.EndedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        #region Helpers

        public async Task<LiveRoom> GetRoomByCodeAsync(string roomCode)
        {
            var room = await _context.LiveRooms
                .Include(lr => lr.Quiz)
                .Include(lr => lr.Players)
                    .ThenInclude(p => p.User)
                .Include(lr => lr.Answers)
                .FirstOrDefaultAsync(lr => lr.RoomCode == roomCode && lr.IsActive);

            if (room == null)
                throw new KeyNotFoundException("Room not found");

            return room;
        }

        private string GenerateUniqueRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code;
            bool isUnique;

            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                isUnique = !_context.LiveRooms.Any(lr => lr.RoomCode == code);
            }
            while (!isUnique);

            return code;
        }

        private bool ValidateAnswer(Question question, object userAnswer)
        {
            try
            {
                if (userAnswer == null)
                {
                    return false;
                }

                switch (question.Type)
                {
                    case QuestionType.SingleChoice:
                    case QuestionType.TrueFalse:
                        var answerString = userAnswer?.ToString();

                        if (string.IsNullOrEmpty(answerString))
                        {
                            return false;
                        }

                        if (int.TryParse(answerString, out var singleAnswer))
                        {
                            var isCorrect = question.AnswerOptions.Any(ao => ao.Id == singleAnswer && ao.IsCorrect);
                            return isCorrect;
                        }
                        return false;

                    case QuestionType.MultipleChoice:
                        var multipleAnswerString = userAnswer?.ToString();

                        if (string.IsNullOrEmpty(multipleAnswerString) || multipleAnswerString == "[]")
                        {
                            return false;
                        }

                        try
                        {
                            var multipleAnswers = JsonSerializer.Deserialize<int[]>(multipleAnswerString);
                            if (multipleAnswers == null || multipleAnswers.Length == 0)
                            {
                                return false;
                            }

                            var correctAnswers = question.AnswerOptions.Where(ao => ao.IsCorrect).Select(ao => ao.Id).OrderBy(id => id);
                            var result = multipleAnswers.OrderBy(id => id).SequenceEqual(correctAnswers);
                            return result;
                        }
                        catch (JsonException)
                        {
                            return false;
                        }

                    case QuestionType.FillInTheBlank:
                        var textAnswer = userAnswer?.ToString()?.Trim().ToLower();

                        if (string.IsNullOrEmpty(textAnswer))
                        {
                            return false;
                        }

                        var fillResult = question.TextAnswer?.ToLower() == textAnswer;
                        return fillResult;

                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private LiveRoomDto MapToLiveRoomDto(LiveRoom room)
        {
            return new LiveRoomDto
            {
                Id = room.Id,
                RoomCode = room.RoomCode,
                Name = room.Name,
                QuizTitle = room.Quiz?.Title ?? string.Empty,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players.Count(p => p.LeftAt == null),
                SecondsPerQuestion = room.SecondsPerQuestion,
                CreatedAt = room.CreatedAt,
                StartsAt = room.CreatedAt.AddSeconds(room.StartDelaySeconds),
                HasStarted = room.StartedAt != null,
                HasEnded = room.EndedAt != null
            };
        }

        private static string MapDifficulty(int? difficulty)
        {
            return difficulty switch
            {
                1 => "Easy",
                2 => "Medium",
                3 => "Hard",
                _ => "Unknown"
            };
        }

        private LiveRoomLobbyDto MapToLobbyDto(LiveRoom room, int userId)
        {
            var host = GetCurrentHost(room);

            return new LiveRoomLobbyDto
            {
                RoomCode = room.RoomCode,
                Name = room.Name,
                QuizTitle = room.Quiz?.Title ?? string.Empty,
                QuizDescription = room.Quiz?.Description ?? string.Empty,
                Difficulty = MapDifficulty(room.Quiz?.Difficulty),
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players.Count(p => p.LeftAt == null),
                SecondsPerQuestion = room.SecondsPerQuestion,
                TimeUntilStart = room.StartedAt.HasValue
                    ? (int)(room.StartedAt.Value - DateTime.UtcNow).TotalSeconds
                    : (int)(room.CreatedAt.AddSeconds(room.StartDelaySeconds) - DateTime.UtcNow).TotalSeconds,
                Players = room.Players
                    .Where(p => p.LeftAt == null && p.User != null)
                    .Select(p => new PlayerDto
                    {
                        UserId = p.UserId,
                        Username = p.User.Username,
                        JoinedAt = p.JoinedAt
                    })
                    .ToList(),
                IsHost = host != null && host.UserId == userId
            };
        }

        public LiveRoomLobbyDto MapToLobbyDtoForOthers(LiveRoom room)
        {
            return new LiveRoomLobbyDto
            {
                RoomCode = room.RoomCode,
                Name = room.Name,
                QuizTitle = room.Quiz?.Title ?? string.Empty,
                QuizDescription = room.Quiz?.Description ?? string.Empty,
                Difficulty = MapDifficulty(room.Quiz?.Difficulty),
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players.Count(p => p.LeftAt == null),
                SecondsPerQuestion = room.SecondsPerQuestion,
                TimeUntilStart = room.StartedAt.HasValue
                    ? (int)(room.StartedAt.Value - DateTime.UtcNow).TotalSeconds
                    : (int)(room.CreatedAt.AddSeconds(room.StartDelaySeconds) - DateTime.UtcNow).TotalSeconds,
                Players = room.Players
                    .Where(p => p.LeftAt == null && p.User != null)
                    .Select(p => new PlayerDto
                    {
                        UserId = p.UserId,
                        Username = p.User.Username,
                        JoinedAt = p.JoinedAt
                    })
                    .ToList(),
                IsHost = false
            };
        }

        public LiveRoomLobbyDto MapToLobbyDtoForCaller(LiveRoom room, int userId, bool isHost)
        {
            return new LiveRoomLobbyDto
            {
                RoomCode = room.RoomCode,
                Name = room.Name,
                QuizTitle = room.Quiz?.Title ?? string.Empty,
                QuizDescription = room.Quiz?.Description ?? string.Empty,
                Difficulty = MapDifficulty(room.Quiz?.Difficulty),
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.Players.Count(p => p.LeftAt == null),
                SecondsPerQuestion = room.SecondsPerQuestion,
                TimeUntilStart = room.StartedAt.HasValue
                    ? (int)(room.StartedAt.Value - DateTime.UtcNow).TotalSeconds
                    : (int)(room.CreatedAt.AddSeconds(room.StartDelaySeconds) - DateTime.UtcNow).TotalSeconds,
                Players = room.Players
                    .Where(p => p.LeftAt == null && p.User != null)
                    .Select(p => new PlayerDto
                    {
                        UserId = p.UserId,
                        Username = p.User.Username,
                        JoinedAt = p.JoinedAt
                    })
                    .ToList(),
                IsHost = isHost
            };
        }

        private LiveRoomPlayer? GetCurrentHost(LiveRoom room)
        {
            return room.Players
                       .Where(p => p.LeftAt == null)
                       .OrderBy(p => p.JoinedAt)
                       .FirstOrDefault();
        }

        public async Task<bool> HaveAllPlayersAnsweredAsync(string roomCode, int questionId)
        {
            var room = await GetRoomByCodeAsync(roomCode);
            var activePlayers = room.Players.Count(p => p.LeftAt == null);

            var answerCount = await _context.LiveRoomAnswers
                .Where(a => a.LiveRoomId == room.Id && a.QuestionId == questionId)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            return activePlayers > 0 && answerCount >= activePlayers;
        }

        #endregion
    }
}