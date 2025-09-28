using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class LiveRoomDto
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string QuizTitle { get; set; } = null!;
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int SecondsPerQuestion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartsAt { get; set; } // CreatedAt + StartDelaySeconds
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
    }
}
