namespace QuizHub.Api.DTOs.QuizResults
{
    public class QuestionResultDto
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
