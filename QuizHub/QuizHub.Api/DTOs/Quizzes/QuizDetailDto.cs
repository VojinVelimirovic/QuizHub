using QuizHub.Api.DTOs.Questions;

namespace QuizHub.Api.DTOs.Quizzes
{
    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy";
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}
