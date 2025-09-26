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
                .Include(q => q.Questions.Where(q => q.IsActive))
                .Where(q => q.IsActive) // Only include active quizzes
                .Where(q => q.Questions.Any(q => q.IsActive)) // Only include quizzes with active questions
                .ToListAsync();

            return _mapper.Map<List<QuizResponseServiceDto>>(quizzes);
        }

        public async Task<QuizDetailServiceDto> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Questions.Where(q => q.IsActive))  // Filter active questions
                    .ThenInclude(qt => qt.AnswerOptions.Where(ao => ao.IsActive))  // Filter active answers
                .Where(q => q.IsActive) // Only return active quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            return new QuizDetailServiceDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CategoryId = quiz.CategoryId,
                CategoryName = quiz.Category.Name,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                Difficulty = quiz.Difficulty == 1 ? "Easy" :
                 quiz.Difficulty == 2 ? "Medium" : "Hard",
                Questions = quiz.Questions.Select(q => new QuestionServiceDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    QuestionType = q.Type.ToString(),
                    TextAnswer = q.Type == QuestionType.FillInTheBlank ? q.TextAnswer : null,
                    AnswerOptions = q.Type != QuestionType.FillInTheBlank
                        ? q.AnswerOptions.Select(ao => new AnswerOptionServiceDto
                        {
                            Id = ao.Id,
                            Text = ao.Text,
                            IsCorrect = ao.IsCorrect
                        }).ToList()
                        : new List<AnswerOptionServiceDto>()
                }).ToList()
            };
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
            quiz.IsActive = true; // New quizzes are always active
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizResponseServiceDto>(quiz);
        }

        public async Task<QuizResultResponseServiceDto> SubmitQuizAsync(int userId, QuizResultCreateServiceDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.Where(q => q.IsActive))  // Only active questions
                    .ThenInclude(qt => qt.AnswerOptions.Where(ao => ao.IsActive))  // Only active answers
                .Where(q => q.IsActive) // Only allow taking active quizzes
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found or is inactive");

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
                var question = quiz.Questions.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
                if (question == null) continue; // Skip if question is inactive

                var correctOptionIds = question.AnswerOptions.Where(a => a.IsCorrect).Select(a => a.Id).ToList();

                bool isCorrect = false;
                string? correctTextAnswer = null;

                switch (question.Type)
                {
                    case QuestionType.FillInTheBlank:
                        correctTextAnswer = question.TextAnswer;
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
                    .ThenInclude(q => q.Questions.Where(q => q.IsActive))  // Only active questions
                        .ThenInclude(q => q.AnswerOptions.Where(ao => ao.IsActive))  // Only active answers
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerOption)
                .Where(r => r.UserId == userId)
                .Where(r => r.Quiz.IsActive) // Only include results from active quizzes
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

        public async Task<List<LeaderboardEntryDto>> GetQuizLeaderboardAsync(int quizId, int top = 10, string timeFilter = "all", int? currentUserId = null)
        {
            var query = _context.QuizResults
                .Include(r => r.User)
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions.Where(q => q.IsActive))  // Only active questions for count
                .Where(r => r.QuizId == quizId)
                .Where(r => r.Quiz.IsActive); // Only include results from active quizzes

            query = timeFilter.ToLower() switch
            {
                "day" => query.Where(r => r.CompletedAt >= DateTime.UtcNow.AddDays(-1)),
                "week" => query.Where(r => r.CompletedAt >= DateTime.UtcNow.AddDays(-7)),
                "month" => query.Where(r => r.CompletedAt >= DateTime.UtcNow.AddDays(-30)),
                _ => query
            };

            var results = await query
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Duration)
                .ThenBy(r => r.CompletedAt)
                .Take(top)
                .Select(r => new LeaderboardEntryDto
                {
                    Username = r.User.Username,
                    CorrectAnswers = r.Score,
                    TotalQuestions = r.Quiz.Questions.Count(q => q.IsActive),  // Count only active questions
                    ScorePercentage = r.Percentage,
                    CompletedAt = r.CompletedAt,
                    Duration = r.Duration,
                    IsCurrentUser = currentUserId.HasValue && r.UserId == currentUserId.Value
                })
                .ToListAsync();

            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
            }

            return results;
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
                    IsActive = true, // New quizzes are always active
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
                        IsActive = true,  // New questions are always active
                        AnswerOptions = qDto.AnswerOptions?.Select(aoDto => new AnswerOption
                        {
                            Text = aoDto.Text,
                            IsCorrect = aoDto.IsCorrect,
                            IsActive = true  // New answers are always active
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

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            // Soft delete - set IsActive to false instead of removing
            quiz.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<QuizResponseServiceDto> UpdateFullQuizAsync(int quizId, QuizFullUpdateServiceDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(qt => qt.AnswerOptions)
                    .Where(q => q.IsActive) // Only update active quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null) throw new KeyNotFoundException("Quiz not found or is inactive");

                // Update quiz basic info
                quiz.Title = dto.Title;
                quiz.Description = dto.Description;
                quiz.CategoryId = dto.CategoryId;
                quiz.TimeLimitMinutes = dto.TimeLimitMinutes;
                quiz.Difficulty = dto.Difficulty?.ToLower() switch
                {
                    "easy" => 1,
                    "medium" => 2,
                    "hard" => 3,
                    _ => 1
                };

                // 1️⃣ Handle deleted answer options - SOFT DELETE
                if (dto.DeletedAnswerIds != null && dto.DeletedAnswerIds.Any())
                {
                    var answersToDeactivate = await _context.AnswerOptions
                        .Where(ao => dto.DeletedAnswerIds.Contains(ao.Id))
                        .ToListAsync();

                    foreach (var answer in answersToDeactivate)
                    {
                        answer.IsActive = false;
                    }
                }

                var existingQuestionIds = quiz.Questions.Select(q => q.Id).ToHashSet();

                foreach (var qDto in dto.Questions)
                {
                    Question question;

                    if (qDto.Id.HasValue && existingQuestionIds.Contains(qDto.Id.Value))
                    {
                        // Update existing question
                        question = quiz.Questions.First(q => q.Id == qDto.Id.Value);
                        question.Text = qDto.Text;
                        question.Type = qDto.QuestionType switch
                        {
                            "SingleChoice" => QuestionType.SingleChoice,
                            "MultipleChoice" => QuestionType.MultipleChoice,
                            "TrueFalse" => QuestionType.TrueFalse,
                            "FillInTheBlank" => QuestionType.FillInTheBlank,
                            _ => QuestionType.SingleChoice
                        };
                        question.TextAnswer = qDto.QuestionType == "FillInTheBlank" ? qDto.TextAnswer : null;
                        question.IsActive = true; // Ensure it's active

                        var existingOptionIds = question.AnswerOptions.Select(o => o.Id).ToHashSet();

                        foreach (var aoDto in qDto.AnswerOptions)
                        {
                            if (aoDto.Id.HasValue && existingOptionIds.Contains(aoDto.Id.Value))
                            {
                                var ao = question.AnswerOptions.First(o => o.Id == aoDto.Id.Value);
                                ao.Text = aoDto.Text;
                                ao.IsCorrect = aoDto.IsCorrect;
                                ao.IsActive = true; // Reactivate if it was previously deactivated
                            }
                            else
                            {
                                question.AnswerOptions.Add(new AnswerOption
                                {
                                    Text = aoDto.Text,
                                    IsCorrect = aoDto.IsCorrect,
                                    IsActive = true
                                });
                            }
                        }

                        // Soft delete answer options not in DTO (skip new options with Id = 0)
                        var dtoOptionIds = qDto.AnswerOptions.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();
                        var optionsToDeactivate = question.AnswerOptions
                            .Where(o => o.Id != 0 && !dtoOptionIds.Contains(o.Id))
                            .ToList();

                        foreach (var option in optionsToDeactivate)
                        {
                            option.IsActive = false;
                        }
                    }
                    else
                    {
                        // Add new question
                        question = new Question
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
                            TextAnswer = qDto.QuestionType == "FillInTheBlank" ? qDto.TextAnswer : null,
                            IsActive = true,
                            AnswerOptions = qDto.AnswerOptions.Select(aoDto => new AnswerOption
                            {
                                Text = aoDto.Text,
                                IsCorrect = aoDto.IsCorrect,
                                IsActive = true
                            }).ToList()
                        };
                        quiz.Questions.Add(question);
                    }
                }

                // Soft delete questions not in DTO (skip new questions with Id = 0)
                var dtoQuestionIds = dto.Questions.Where(q => q.Id.HasValue).Select(q => q.Id!.Value).ToHashSet();
                var questionsToDeactivate = quiz.Questions
                    .Where(q => q.Id != 0 && !dtoQuestionIds.Contains(q.Id))
                    .ToList();

                foreach (var question in questionsToDeactivate)
                {
                    question.IsActive = false;
                    // Also deactivate all answer options for this question
                    foreach (var option in question.AnswerOptions)
                    {
                        option.IsActive = false;
                    }
                }

                _context.Quizzes.Update(quiz);
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