using Microsoft.AspNetCore.Mvc;
using QuizHub.Api.DTOs.Users;
using QuizHub.Services.DTOs.Users;
using QuizHub.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace QuizHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public UsersController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserCreateDto userDto, IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
                return BadRequest(new { message = "Profile picture is required." });

            string uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "images/profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            string profileImageUrl = $"/images/profiles/{uniqueFileName}";

            try
            {
                var user = await _userService.RegisterAsync(userDto, profileImageUrl);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginServiceDto loginDto)
        {
            try
            {
                var (token, user) = await _userService.LoginAsync(loginDto);
                return Ok(new { token, user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
