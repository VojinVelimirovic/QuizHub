namespace QuizHub.Api.DTOs.QuizResults
{
    public class QuizResultResponseDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();
    }
}
