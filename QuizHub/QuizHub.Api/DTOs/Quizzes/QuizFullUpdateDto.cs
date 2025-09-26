using System.Collections.Generic;
using QuizHub.Api.DTOs.Questions;

namespace QuizHub.Api.DTOs.Quizzes
{
    public class QuizFullUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy";

        public List<QuestionUpdateDto> Questions { get; set; } = new List<QuestionUpdateDto>();
        public List<int> DeletedAnswerIds { get; set; } = new();
    }
}
