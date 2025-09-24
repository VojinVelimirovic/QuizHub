using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Context;
using QuizHub.Data.Models;
using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.DTOs.Questions;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizHub.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<QuizResponseServiceDto>> GetAllQuizzesAsync()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Questions)
                .ToListAsync();

            return _mapper.Map<List<QuizResponseServiceDto>>(quizzes);
        }

        public async Task<QuizDetailServiceDto> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            return _mapper.Map<QuizDetailServiceDto>(quiz);
        }

        public async Task<QuizResponseServiceDto> CreateQuizAsync(QuizCreateServiceDto dto)
        {
            var quiz = _mapper.Map<Quiz>(dto);
            quiz.Difficulty = dto.Difficulty?.ToLower() switch
            {
                "easy" => 1,
                "medium" => 2,
                "hard" => 3,
                _ => 1
            };
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizResponseServiceDto>(quiz);
        }

        public async Task<QuizResultResponseServiceDto> SubmitQuizAsync(int userId, QuizResultCreateServiceDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            int correctCount = 0;
            var questionResults = new List<QuestionResultServiceDto>();

            foreach (var userAnswer in dto.Answers)
            {
                var question = quiz.Questions.First(q => q.Id == userAnswer.QuestionId);
                var correctOptionIds = question.AnswerOptions.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                bool isCorrect = false;

                if (question.Type == QuestionType.FillInTheBlank)
                    isCorrect = userAnswer.TextAnswer?.Trim().ToLower() == correctOptionIds.FirstOrDefault().ToString();
                else
                    isCorrect = userAnswer.SelectedAnswerIds.OrderBy(x => x).SequenceEqual(correctOptionIds.OrderBy(x => x));

                if (isCorrect) correctCount++;

                questionResults.Add(new QuestionResultServiceDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    IsCorrect = isCorrect,
                    SelectedAnswerIds = userAnswer.SelectedAnswerIds,
                    CorrectAnswerIds = correctOptionIds,
                    UserTextAnswer = userAnswer.TextAnswer,
                    CorrectTextAnswer = null
                });
            }

            double percentage = ((double)correctCount / quiz.Questions.Count) * 100;

            var result = new QuizResult
            {
                UserId = userId,
                QuizId = quiz.Id,
                Score = correctCount,
                Percentage = percentage,
                CompletedAt = DateTime.UtcNow,
                Duration = TimeSpan.FromSeconds(dto.DurationSeconds)
            };

            _context.QuizResults.Add(result);
            await _context.SaveChangesAsync();

            return new QuizResultResponseServiceDto
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TotalQuestions = quiz.Questions.Count,
                CorrectAnswers = correctCount,
                ScorePercentage = percentage,
                QuestionResults = questionResults
            };
        }
        public async Task<List<QuizResultResponseServiceDto>> GetUserResultsAsync(int userId)
        {
            var results = await _context.QuizResults
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            var mappedResults = new List<QuizResultResponseServiceDto>();

            foreach (var result in results)
            {
                var questionResults = result.Quiz.Questions.Select(q => new QuestionResultServiceDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    IsCorrect = true,
                    SelectedAnswerIds = new List<int>(),
                    CorrectAnswerIds = q.AnswerOptions.Where(a => a.IsCorrect).Select(a => a.Id).ToList(),
                    UserTextAnswer = null,
                    CorrectTextAnswer = null
                }).ToList();

                mappedResults.Add(new QuizResultResponseServiceDto
                {
                    QuizId = result.QuizId,
                    QuizTitle = result.Quiz.Title,
                    TotalQuestions = result.Quiz.Questions.Count,
                    CorrectAnswers = result.Score,
                    ScorePercentage = result.Percentage,
                    QuestionResults = questionResults,
                    CompletedAt = result.CompletedAt,
                    Duration = result.Duration
                });
            }

            return mappedResults;
        }

        public async Task<List<QuizResultResponseServiceDto>> GetQuizLeaderboardAsync(int quizId, int top = 10)
        {
            var results = await _context.QuizResults
                .Include(r => r.User)
                .Include(r => r.Quiz)
                .Where(r => r.QuizId == quizId)
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.CompletedAt)
                .Take(top)
                .ToListAsync();

            var leaderboard = new List<QuizResultResponseServiceDto>();

            foreach (var result in results)
            {
                leaderboard.Add(new QuizResultResponseServiceDto
                {
                    QuizId = result.QuizId,
                    QuizTitle = result.Quiz.Title,
                    TotalQuestions = result.Quiz.Questions.Count,
                    CorrectAnswers = result.Score,
                    ScorePercentage = result.Percentage,
                    QuestionResults = new List<QuestionResultServiceDto>(),
                    CompletedAt = result.CompletedAt,
                    Duration = result.Duration
                });
            }

            return leaderboard;
        }

        public async Task<QuizResponseServiceDto> CreateFullQuizAsync(QuizFullCreateServiceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Quiz title is required.");

            if (dto.Questions == null || !dto.Questions.Any())
                throw new InvalidOperationException("At least one question is required.");

            foreach (var qDto in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(qDto.Text))
                    throw new InvalidOperationException("Question text is required.");

                if (qDto.QuestionType == "FillIn")
                {
                    if (string.IsNullOrWhiteSpace(qDto.TextAnswer))
                        throw new InvalidOperationException("Fill-in questions must have an answer.");
                }
                else
                {
                    if (qDto.AnswerOptions == null || !qDto.AnswerOptions.Any())
                        throw new InvalidOperationException("Multiple choice / true-false questions must have answer options.");

                    if (!qDto.AnswerOptions.Any(ao => ao.IsCorrect))
                        throw new InvalidOperationException("Each question must have at least one correct answer.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var quiz = new Quiz
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    TimeLimitMinutes = dto.TimeLimitMinutes,
                    Difficulty = dto.Difficulty?.ToLower() switch
                    {
                        "easy" => 1,
                        "medium" => 2,
                        "hard" => 3,
                        _ => 1
                    },
                    Questions = dto.Questions.Select(qDto => new Question
                    {
                        Text = qDto.Text,
                        Type = qDto.QuestionType switch
                        {
                            "SingleChoice" => QuestionType.SingleChoice,
                            "MultipleChoice" => QuestionType.MultipleChoice,
                            "TrueFalse" => QuestionType.TrueFalse,
                            "FillIn" => QuestionType.FillInTheBlank,
                            _ => QuestionType.SingleChoice
                        },
                        Points = qDto.Points,
                        AnswerOptions = qDto.AnswerOptions?.Select(aoDto => new AnswerOption
                        {
                            Text = aoDto.Text,
                            IsCorrect = aoDto.IsCorrect
                        }).ToList() ?? new List<AnswerOption>(),
                        TextAnswer = qDto.QuestionType == "FillIn" ? qDto.TextAnswer : null
                    }).ToList()
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return _mapper.Map<QuizResponseServiceDto>(quiz);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
