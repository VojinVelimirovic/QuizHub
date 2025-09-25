using System.Collections.Generic;

namespace QuizHub.Services.DTOs.Questions
{
    public class QuestionCreateServiceDto
    {
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice, MultipleChoice, TrueFalse, FillIn

        public List<AnswerOptionCreateServiceDto> AnswerOptions { get; set; } = new List<AnswerOptionCreateServiceDto>();
        public string? TextAnswer { get; set; }
    }
}