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

            int score = 0;
            int totalQuestions = quiz.Questions.Count;

            var questionResults = new List<QuestionResultServiceDto>();

            var quizResult = new QuizResult
            {
                UserId = userId,
                QuizId = quiz.Id,
                CompletedAt = DateTime.UtcNow,
                Duration = TimeSpan.FromSeconds(dto.DurationSeconds),
            };

            _context.QuizResults.Add(quizResult);
            await _context.SaveChangesAsync();

            foreach (var userAnswer in dto.Answers)
            {
                var question = quiz.Questions.First(q => q.Id == userAnswer.QuestionId);
                var correctOptionIds = question.AnswerOptions.Where(a => a.IsCorrect).Select(a => a.Id).ToList();

                bool isCorrect = false;
                string? correctTextAnswer = null;

                switch (question.Type)
                {
                    case QuestionType.FillInTheBlank:
                        correctTextAnswer = question.AnswerOptions.FirstOrDefault(a => a.IsCorrect)?.Text;
                        isCorrect = string.Equals(
                            userAnswer.TextAnswer?.Trim(),
                            correctTextAnswer?.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        );

                        _context.QuizResultAnswers.Add(new QuizResultAnswer
                        {
                            QuizResultId = quizResult.Id,
                            QuestionId = question.Id,
                            TextAnswer = userAnswer.TextAnswer
                        });
                        break;

                    case QuestionType.SingleChoice:
                    case QuestionType.TrueFalse:
                        isCorrect = userAnswer.SelectedAnswerIds
                            .OrderBy(x => x)
                            .SequenceEqual(correctOptionIds.OrderBy(x => x));

                        if (userAnswer.SelectedAnswerIds.Any())
                        {
                            _context.QuizResultAnswers.Add(new QuizResultAnswer
                            {
                                QuizResultId = quizResult.Id,
                                QuestionId = question.Id,
                                AnswerOptionId = userAnswer.SelectedAnswerIds.First()
                            });
                        }
                        break;

                    case QuestionType.MultipleChoice:
                        isCorrect = userAnswer.SelectedAnswerIds
                            .OrderBy(x => x)
                            .SequenceEqual(correctOptionIds.OrderBy(x => x));

                        foreach (var selectedId in userAnswer.SelectedAnswerIds)
                        {
                            _context.QuizResultAnswers.Add(new QuizResultAnswer
                            {
                                QuizResultId = quizResult.Id,
                                QuestionId = question.Id,
                                AnswerOptionId = selectedId
                            });
                        }
                        break;
                }

                if (isCorrect)
                    score++;

                questionResults.Add(new QuestionResultServiceDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    IsCorrect = isCorrect,
                    SelectedAnswerIds = userAnswer.SelectedAnswerIds,
                    CorrectAnswerIds = correctOptionIds,
                    UserTextAnswer = userAnswer.TextAnswer,
                    CorrectTextAnswer = correctTextAnswer
                });
            }

            quizResult.Score = score;
            quizResult.Percentage = totalQuestions > 0 ? ((double)score / totalQuestions) * 100 : 0;

            await _context.SaveChangesAsync();

            return new QuizResultResponseServiceDto
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TotalQuestions = totalQuestions,
                CorrectAnswers = score,
                ScorePercentage = quizResult.Percentage,
                CompletedAt = quizResult.CompletedAt,
                Duration = quizResult.Duration,
                QuestionResults = questionResults
            };
        }



        public async Task<List<QuizResultResponseServiceDto>> GetUserResultsAsync(int userId)
        {
            var results = await _context.QuizResults
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerOption)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .AsNoTracking()
                .ToListAsync();

            var mappedResults = new List<QuizResultResponseServiceDto>();

            foreach (var result in results)
            {
                var questionResults = new List<QuestionResultServiceDto>();

                foreach (var question in result.Quiz.Questions)
                {
                    var userAnswers = result.Answers.Where(a => a.QuestionId == question.Id).ToList();
                    var selectedIds = userAnswers.Where(a => a.AnswerOptionId.HasValue)
                                                 .Select(a => a.AnswerOptionId!.Value)
                                                 .ToList();
                    var userTextAnswer = userAnswers.FirstOrDefault(a => a.TextAnswer != null)?.TextAnswer;

                    var correctOptionIds = question.AnswerOptions.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                    var correctTextAnswer = question.Type == QuestionType.FillInTheBlank
                        ? question.AnswerOptions.FirstOrDefault(a => a.IsCorrect)?.Text
                        : null;

                    bool isCorrect = false;
                    switch (question.Type)
                    {
                        case QuestionType.FillInTheBlank:
                            isCorrect = string.Equals(
                                userTextAnswer?.Trim() ?? "",
                                correctTextAnswer?.Trim() ?? "",
                                StringComparison.OrdinalIgnoreCase
                            );
                            break;
                        default:
                            isCorrect = selectedIds.OrderBy(x => x).SequenceEqual(correctOptionIds.OrderBy(x => x));
                            break;
                    }
                    var selectedAnswerTexts = question.AnswerOptions
                        .Where(a => selectedIds.Contains(a.Id))
                        .Select(a => a.Text ?? "")
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();

                    var correctAnswerTexts = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text ?? "")
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();

                    questionResults.Add(new QuestionResultServiceDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        SelectedAnswerIds = selectedIds,
                        SelectedAnswerTexts = selectedAnswerTexts,
                        CorrectAnswerIds = correctOptionIds,
                        CorrectAnswerTexts = correctAnswerTexts,
                        UserTextAnswer = userTextAnswer,
                        CorrectTextAnswer = correctTextAnswer
                    });
                }

                mappedResults.Add(new QuizResultResponseServiceDto
                {
                    QuizId = result.QuizId,
                    QuizTitle = result.Quiz.Title,
                    TotalQuestions = result.Quiz.Questions.Count,
                    CorrectAnswers = result.Score,
                    ScorePercentage = result.Percentage,
                    CompletedAt = result.CompletedAt,
                    Duration = result.Duration,
                    QuestionResults = questionResults
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

                if (qDto.QuestionType == "FillInTheBlank")
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
                            "FillInTheBlank" => QuestionType.FillInTheBlank,
                            _ => QuestionType.SingleChoice
                        },
                        AnswerOptions = qDto.AnswerOptions?.Select(aoDto => new AnswerOption
                        {
                            Text = aoDto.Text,
                            IsCorrect = aoDto.IsCorrect
                        }).ToList() ?? new List<AnswerOption>(),
                        TextAnswer = qDto.QuestionType == "FillInTheBlank" ? qDto.TextAnswer : null
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
