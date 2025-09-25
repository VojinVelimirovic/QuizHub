using System.Collections.Generic;

namespace QuizHub.Api.DTOs.Questions
{
    public class QuestionCreateDto
    {
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice, MultipleChoice, TrueFalse, FillIn

        public List<AnswerOptionCreateDto> AnswerOptions { get; set; } = new List<AnswerOptionCreateDto>();
        public string? TextAnswer { get; set; }
    }
}