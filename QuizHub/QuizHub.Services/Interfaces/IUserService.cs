using QuizHub.Services.DTOs.Users;
using System.Threading.Tasks;

namespace QuizHub.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResultDto> RegisterAsync(UserCreateDto userDto, string? profileImageUrl);
        Task<(string Token, UserResultDto User)> LoginAsync(UserLoginServiceDto loginDto);
    }
}
