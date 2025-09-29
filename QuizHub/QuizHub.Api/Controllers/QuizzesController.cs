using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.Interfaces;
using QuizHub.Api.DTOs.Quizzes;
using QuizHub.Services.DTOs.Questions;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuizCreateServiceDto dto)
        {
            var quiz = await _quizService.CreateQuizAsync(dto);
            return Ok(quiz);
        }

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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _quizService.DeleteQuizAsync(id);
                return NoContent();
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

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/full")]
        public async Task<IActionResult> PatchFull(int id, [FromBody] QuizFullUpdateDto dto)
        {
            try
            {
                var serviceDto = new QuizFullUpdateServiceDto
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    TimeLimitMinutes = dto.TimeLimitMinutes,
                    Difficulty = dto.Difficulty,
                    DeletedAnswerIds = dto.DeletedAnswerIds,
                    Questions = dto.Questions.Select(q => new QuestionUpdateServiceDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        QuestionType = q.QuestionType,
                        AnswerOptions = q.AnswerOptions.Select(ao => new AnswerOptionUpdateServiceDto
                        {
                            Id = ao.Id,
                            Text = ao.Text,
                            IsCorrect = ao.IsCorrect
                        }).ToList(),
                        TextAnswer = q.QuestionType == "FillInTheBlank" ? q.TextAnswer : null
                    }).ToList()
                };

                var updated = await _quizService.UpdateFullQuizAsync(id, serviceDto);
                return Ok(updated);
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
    }
}