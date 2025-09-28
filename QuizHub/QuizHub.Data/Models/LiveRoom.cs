using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public class LiveRoom
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } // Unique code for joining
        public string Name { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int MaxPlayers { get; set; }
        public int SecondsPerQuestion { get; set; }
        public int StartDelaySeconds { get; set; } // Time before game starts
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int CurrentQuestionIndex { get; set; } = -1; // -1 = lobby, 0+ = question index
        public ICollection<LiveRoomPlayer> Players { get; set; } = new List<LiveRoomPlayer>();
        public ICollection<LiveRoomAnswer> Answers { get; set; } = new List<LiveRoomAnswer>();
    }
}
