using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public class AnswerOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsCorrect { get; set; }
        public Question Question { get; set; } = null!;
    }

}
