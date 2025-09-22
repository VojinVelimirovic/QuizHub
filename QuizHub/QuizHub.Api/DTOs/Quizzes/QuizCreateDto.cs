namespace QuizHub.Api.DTOs.Quizzes
{
    public class QuizCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy"; // Easy, Medium, Hard
    }
}
