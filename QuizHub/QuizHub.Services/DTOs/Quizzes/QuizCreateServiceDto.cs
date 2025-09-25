namespace QuizHub.Services.DTOs.Quizzes
{
    public class QuizCreateServiceDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy";
    }
}
