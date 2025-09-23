namespace QuizHub.Services.DTOs.Questions
{
    public class AnswerOptionCreateServiceDto
    {
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}