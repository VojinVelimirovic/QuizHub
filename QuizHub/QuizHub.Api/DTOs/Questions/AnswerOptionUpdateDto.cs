namespace QuizHub.Api.DTOs.Questions
{
    public class AnswerOptionUpdateDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
