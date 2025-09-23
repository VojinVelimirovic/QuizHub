using System.Collections.Generic;

namespace QuizHub.Api.DTOs.Questions
{
    public class QuestionCreateDto
    {
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice, MultipleChoice, TrueFalse, FillIn
        public int Points { get; set; } = 1;

        public List<AnswerOptionCreateDto> AnswerOptions { get; set; } = new List<AnswerOptionCreateDto>();
    }
}