using QuizHub.Services.DTOs.Questions;
using System.Collections.Generic;

namespace QuizHub.Services.DTOs.Quizzes
{
    public class QuizDetailServiceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = null!;
        public List<QuestionServiceDto> Questions { get; set; } = new List<QuestionServiceDto>();
    }
}
