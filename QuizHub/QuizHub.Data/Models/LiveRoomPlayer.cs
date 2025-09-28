using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public class LiveRoomPlayer
    {
        public int Id { get; set; }
        public int LiveRoomId { get; set; }
        public LiveRoom LiveRoom { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public int Score { get; set; } = 0;
        public bool IsReady { get; set; } = false;
    }
}
