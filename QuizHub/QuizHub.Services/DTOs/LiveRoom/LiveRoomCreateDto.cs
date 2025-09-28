using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class LiveRoomCreateDto
    {
        public string Name { get; set; } = null!;
        public int QuizId { get; set; }
        public int MaxPlayers { get; set; }
        public int SecondsPerQuestion { get; set; }
        public int StartDelaySeconds { get; set; }
    }
}
