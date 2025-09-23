using System.Collections.Generic;

namespace QuizHub.Services.DTOs.QuizResults
{
    public class QuizResultResponseServiceDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public List<QuestionResultServiceDto> QuestionResults { get; set; } = new List<QuestionResultServiceDto>();
    }
}
