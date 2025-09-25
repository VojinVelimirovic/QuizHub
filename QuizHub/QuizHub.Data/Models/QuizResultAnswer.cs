namespace QuizHub.Data.Models
{
    public class QuizResultAnswer
    {
        public int Id { get; set; }
        public int QuizResultId { get; set; }
        public int QuestionId { get; set; }
        public int? AnswerOptionId { get; set; }
        public string? TextAnswer { get; set; }

        public QuizResult QuizResult { get; set; } = null!;
        public AnswerOption? AnswerOption { get; set; }
    }
}
