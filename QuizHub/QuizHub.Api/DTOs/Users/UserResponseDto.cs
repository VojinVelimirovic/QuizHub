namespace QuizHub.Api.DTOs.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        public string Role { get; set; } = "User";
    }
}
