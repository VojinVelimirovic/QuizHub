using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.Interfaces;
using System;
using System.Threading.Tasks;
using QuizHub.Api.DTOs.Quizzes;
using QuizHub.Services.DTOs.Questions;
using QuizHub.Api.DTOs.Categories;
using QuizHub.Services.DTOs.Categories;

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
                var userId = int.Parse(User.FindFirst("sub")!.Value); // JWT sub claim
                var result = await _quizService.SubmitQuizAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Quiz not found" });
            }
        }

        // GET: api/quizzes/user-results
        [Authorize]
        [HttpGet("user-results")]
        public async Task<IActionResult> GetUserResults()
        {
            var userId = int.Parse(User.FindFirst("sub")!.Value);
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
            // Map API DTO → Service DTO
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
                    Points = q.Points,
                    AnswerOptions = q.AnswerOptions.Select(ao => new AnswerOptionCreateServiceDto
                    {
                        Text = ao.Text,
                        IsCorrect = ao.IsCorrect
                    }).ToList()
                }).ToList()
            };

            var quiz = await _quizService.CreateFullQuizAsync(serviceDto);
            return Ok(quiz);
        }
    }
}
