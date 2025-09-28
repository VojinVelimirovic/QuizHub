using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class LiveRoomQuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
        public int TimeRemaining { get; set; }
        public int QuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class AnswerOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
    }
}
