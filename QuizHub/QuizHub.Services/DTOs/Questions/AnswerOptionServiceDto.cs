namespace QuizHub.Services.DTOs.Questions
{
    public class AnswerOptionServiceDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; } // Needed internally in service layer
    }
}
