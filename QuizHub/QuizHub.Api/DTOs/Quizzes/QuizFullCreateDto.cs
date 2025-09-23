using System.Collections.Generic;
using QuizHub.Api.DTOs.Questions;

namespace QuizHub.Api.DTOs.Quizzes
{
    public class QuizFullCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy"; // Easy, Medium, Hard

        public List<QuestionCreateDto> Questions { get; set; } = new List<QuestionCreateDto>();
    }
}