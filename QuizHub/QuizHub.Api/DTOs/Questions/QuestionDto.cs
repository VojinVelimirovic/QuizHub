using QuizHub.Api.DTOs.Questions;

namespace QuizHub.Api.DTOs.Questions
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice, MultipleChoice, TrueFalse, FillIn
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new List<AnswerOptionDto>();
        public int Points { get; set; } = 1; // Default points for the question
    }
}
