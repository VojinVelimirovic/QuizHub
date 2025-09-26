namespace QuizHub.Services.DTOs.QuizResults
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string Username { get; set; } = null!;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}