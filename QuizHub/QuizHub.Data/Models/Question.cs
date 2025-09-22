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
        public QuestionType Type { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    }

}
