namespace QuizHub.Api.DTOs.QuizResults
{
    public class QuizResultCreateDto
    {
        public int QuizId { get; set; }
        public List<UserAnswerDto> Answers { get; set; } = new List<UserAnswerDto>();
        public int DurationSeconds { get; set; }
    }
}
