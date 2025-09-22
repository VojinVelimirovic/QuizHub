namespace QuizHub.Services.DTOs.Users
{
    public class UserLoginServiceDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
