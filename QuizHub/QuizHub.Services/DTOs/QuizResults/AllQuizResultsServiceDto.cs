using System;

namespace QuizHub.Services.DTOs.QuizResults
{
    public class AllQuizResultsServiceDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
    }
}