using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        TrueFalse,
        FillInTheBlank
    }

    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public QuestionType Type { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public string? TextAnswer { get; set; }
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    }

}
