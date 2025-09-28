using QuizHub.Data.Models;
using QuizHub.Services.DTOs.LiveRoom;
using QuizHub.Services.DTOs.QuizResults;

namespace QuizHub.Services.Interfaces
{
    public interface ILiveRoomService
    {
        Task<LiveRoomDto> CreateRoomAsync(LiveRoomCreateDto dto, int hostUserId);
        Task<List<LiveRoomDto>> GetActiveRoomsAsync();
        Task<LiveRoomLobbyDto> JoinRoomAsync(string roomCode, int userId);
        Task<bool> LeaveRoomAsync(string roomCode, int userId);
        Task<LiveRoomLobbyDto> GetLobbyStatusAsync(string roomCode, int userId);
        Task<LiveRoomLobbyDto> GetLobbyStatusForOthersAsync(string roomCode);
        Task<bool> StartRoomAsync(string roomCode, int userId);
        Task<LiveRoomQuestionDto> GetCurrentQuestionAsync(string roomCode);
        Task<bool> SubmitAnswerAsync(string roomCode, int userId, AnswerSubmissionDto submission);
        Task<LiveLeaderboardDto> GetLiveLeaderboardAsync(string roomCode);
        Task<bool> AdvanceQuestionAsync(string roomCode);
        Task<bool> EndRoomAsync(string roomCode);
        Task CleanupExpiredRoomsAsync();
        Task<LiveRoomLobbyDto> GetLobbyStatusExcludingUserAsync(string roomCode, int excludedUserId);

        Task<LiveRoom> GetRoomByCodeAsync(string roomCode);
        LiveRoomLobbyDto MapToLobbyDtoForOthers(LiveRoom room);
        LiveRoomLobbyDto MapToLobbyDtoForCaller(LiveRoom room, int userId, bool isHost);

        Task<bool> HaveAllPlayersAnsweredAsync(string roomCode, int questionId);

    }
}