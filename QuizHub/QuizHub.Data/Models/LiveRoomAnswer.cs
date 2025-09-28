using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public class LiveRoomAnswer
    {
        public int Id { get; set; }
        public int LiveRoomId { get; set; }
        public LiveRoom LiveRoom { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string SubmittedAnswer { get; set; } // JSON for multiple choice, text for fill-in
        public bool IsCorrect { get; set; }
        public DateTime SubmittedAt { get; set; }
        public double ResponseTimeSeconds { get; set; } // For bonus points
        public bool GotFirstBlood { get; set; } = false; // First correct answer
    }
}
