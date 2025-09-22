namespace QuizHub.Api.DTOs.Quizzes
{
    public class QuizResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int QuestionCount { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Difficulty { get; set; } = "Easy";
    }
}
