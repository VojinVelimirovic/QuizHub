using System.Collections.Generic;
using QuizHub.Services.DTOs.Questions;

namespace QuizHub.Services.DTOs.Quizzes
{
    public class QuizFullCreateServiceDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy";

        public List<QuestionCreateServiceDto> Questions { get; set; } = new List<QuestionCreateServiceDto>();
    }
}