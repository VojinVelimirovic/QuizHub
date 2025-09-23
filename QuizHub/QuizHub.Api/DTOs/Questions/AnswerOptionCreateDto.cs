namespace QuizHub.Api.DTOs.Questions
{
    public class AnswerOptionCreateDto
    {
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
