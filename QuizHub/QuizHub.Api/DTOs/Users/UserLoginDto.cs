namespace QuizHub.Api.DTOs.Users
{
    public class UserLoginDto
    {
        public string UsernameOrEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
