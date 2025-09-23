using System.Collections.Generic;

namespace QuizHub.Services.DTOs.QuizResults
{
    public class UserAnswerServiceDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new List<int>();
        public string? TextAnswer { get; set; }
    }
}
