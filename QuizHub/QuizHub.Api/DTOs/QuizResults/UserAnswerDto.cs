namespace QuizHub.Api.DTOs.QuizResults
{
    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new List<int>(); // For multiple choice
        public string? TextAnswer { get; set; } // For fill-in-the-blank
    }
}
