using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class AnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        public object Answer { get; set; }
        public double ClientSubmittedAt { get; set; }
    }
}
