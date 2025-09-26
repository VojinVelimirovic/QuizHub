using System.Collections.Generic;

namespace QuizHub.Api.DTOs.Questions
{
    public class QuestionUpdateDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice";
        public List<AnswerOptionUpdateDto> AnswerOptions { get; set; } = new List<AnswerOptionUpdateDto>();
        public string? TextAnswer { get; set; }
    }
}
