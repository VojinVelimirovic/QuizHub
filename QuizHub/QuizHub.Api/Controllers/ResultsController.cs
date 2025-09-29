using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QuizHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public ResultsController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [Authorize]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] QuizResultCreateServiceDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 User.FindFirst(ClaimTypes.Name);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _quizService.SubmitQuizAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Quiz not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetUserResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                             User.FindFirst(ClaimTypes.Name);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var results = await _quizService.GetUserResultsAsync(userId);
            return Ok(results);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllResults()
        {
            try
            {
                var results = await _quizService.GetAllQuizResultsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("leaderboard/{quizId}")]
        public async Task<IActionResult> GetLeaderboard(int quizId, [FromQuery] int top = 10, [FromQuery] string timeFilter = "all")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                             User.FindFirst(ClaimTypes.Name);

            int? currentUserId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                currentUserId = userId;
            }

            var leaderboard = await _quizService.GetQuizLeaderboardAsync(quizId, top, timeFilter, currentUserId);
            return Ok(leaderboard);
        }
    }
}