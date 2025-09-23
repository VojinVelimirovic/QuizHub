using System.Collections.Generic;

namespace QuizHub.Services.DTOs.QuizResults
{
    public class QuizResultCreateServiceDto
    {
        public int QuizId { get; set; }
        public List<UserAnswerServiceDto> Answers { get; set; } = new List<UserAnswerServiceDto>();
        public int DurationSeconds { get; set; }
    }
}
