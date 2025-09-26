using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.DTOs.QuizResults;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizHub.Services.Interfaces
{
    public interface IQuizService
    {
        Task<List<QuizResponseServiceDto>> GetAllQuizzesAsync();
        Task<QuizDetailServiceDto> GetQuizByIdAsync(int quizId);
        Task<QuizResponseServiceDto> CreateQuizAsync(QuizCreateServiceDto dto);
        Task<QuizResultResponseServiceDto> SubmitQuizAsync(int userId, QuizResultCreateServiceDto dto);
        Task<List<QuizResultResponseServiceDto>> GetUserResultsAsync(int userId);
        Task<List<LeaderboardEntryDto>> GetQuizLeaderboardAsync(int quizId, int top = 10, string timefilter = "all", int? currentUserId = null);
        Task<QuizResponseServiceDto> CreateFullQuizAsync(QuizFullCreateServiceDto dto);
        Task DeleteQuizAsync(int quizId);
        Task<QuizResponseServiceDto> UpdateFullQuizAsync(int quizId, QuizFullUpdateServiceDto dto);

        Task<List<AllQuizResultsServiceDto>> GetAllQuizResultsAsync();

    }
}
