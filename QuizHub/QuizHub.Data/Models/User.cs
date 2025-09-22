using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = "User"; // "User" or "Admin"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    }

}
