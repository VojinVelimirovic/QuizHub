using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.DTOs.LiveRoom
{
    public class LiveLeaderboardDto
    {
        public bool IsFinal { get; set; }
        public List<LiveLeaderboardEntryDto> Entries { get; set; } = new();
    }

    public class LiveLeaderboardEntryDto
    {
        public int Position { get; set; }
        public string Username { get; set; } = null!;
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public double AverageResponseTime { get; set; }
    }
}
