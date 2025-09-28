using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class LiveRoomLobbyDto
    {
        public string RoomCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string QuizTitle { get; set; } = null!;
        public string QuizDescription { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int SecondsPerQuestion { get; set; }
        public int TimeUntilStart { get; set; }
        public List<PlayerDto> Players { get; set; } = new();
        public bool IsHost { get; set; }
    }

    public class PlayerDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
    }
}
