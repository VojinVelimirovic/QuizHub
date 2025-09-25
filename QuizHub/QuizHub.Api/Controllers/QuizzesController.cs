using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Threading.Tasks;
using QuizHub.Api.DTOs.Quizzes;
using QuizHub.Services.DTOs.Questions;
using QuizHub.Api.DTOs.Categories;
using QuizHub.Services.DTOs.Categories;
using System.Security.Claims;

namespace QuizHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/quizzes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        // GET: api/quizzes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(id);
                return Ok(quiz);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Quiz not found" });
            }
        }

        // POST: api/quizzes
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuizCreateServiceDto dto)
        {
            var quiz = await _quizService.CreateQuizAsync(dto);
            return Ok(quiz);
        }

        // POST: api/quizzes/submit
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

        // GET: api/quizzes/user-results
        [Authorize]
        [HttpGet("user-results")]
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

        // GET: api/quizzes/{id}/leaderboard
        [HttpGet("{id}/leaderboard")]
        public async Task<IActionResult> GetLeaderboard(int id, [FromQuery] int top = 10)
        {
            var leaderboard = await _quizService.GetQuizLeaderboardAsync(id, top);
            return Ok(leaderboard);
        }

        // POST: api/quizzes/full
        [Authorize(Roles = "Admin")]
        [HttpPost("full")]
        public async Task<IActionResult> CreateFullQuiz([FromBody] QuizFullCreateDto dto)
        {
            var serviceDto = new QuizFullCreateServiceDto
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                Difficulty = dto.Difficulty,
                Questions = dto.Questions.Select(q => new QuestionCreateServiceDto
                {
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    AnswerOptions = q.AnswerOptions?.Select(ao => new AnswerOptionCreateServiceDto
                    {
                        Text = ao.Text,
                        IsCorrect = ao.IsCorrect
                    }).ToList() ?? new List<AnswerOptionCreateServiceDto>(),

                    TextAnswer = q.QuestionType == "FillInTheBlank" ? q.TextAnswer : null
                }).ToList()
            };

            var quiz = await _quizService.CreateFullQuizAsync(serviceDto);
            return Ok(quiz);
        }
    }
}
