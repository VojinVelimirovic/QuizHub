using System.Collections.Generic;

namespace QuizHub.Services.DTOs.Questions
{
    public class QuestionServiceDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = "SingleChoice";
        public List<AnswerOptionServiceDto> AnswerOptions { get; set; } = new List<AnswerOptionServiceDto>();
    }
}
