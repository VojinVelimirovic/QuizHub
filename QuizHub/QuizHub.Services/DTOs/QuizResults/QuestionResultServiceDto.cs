using System.Collections.Generic;

namespace QuizHub.Services.DTOs.QuizResults
{
    public class QuestionResultServiceDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new List<int>();
        public List<int> CorrectAnswerIds { get; set; } = new List<int>();
        public string? UserTextAnswer { get; set; }
        public string? CorrectTextAnswer { get; set; }
    }
}
