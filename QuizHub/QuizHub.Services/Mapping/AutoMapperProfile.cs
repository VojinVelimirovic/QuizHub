using AutoMapper;
using QuizHub.Data.Models;
using QuizHub.Services.DTOs.Categories;
using QuizHub.Services.DTOs.Questions;
using QuizHub.Services.DTOs.QuizResults;
using QuizHub.Services.DTOs.Quizzes;
using QuizHub.Services.DTOs.Users;

namespace QuizHub.Services.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Users
            CreateMap<UserCreateDto, User>();
            CreateMap<User, UserResultDto>();

            // Quizzes
            CreateMap<QuizCreateServiceDto, Quiz>()
                .ForMember(dest => dest.Difficulty, opt => opt.Ignore());

            CreateMap<Quiz, QuizResponseServiceDto>()
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Quiz, QuizDetailServiceDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Difficulty,
                opt => opt.MapFrom(src =>
                    src.Difficulty == 1 ? "Easy" :
                    src.Difficulty == 2 ? "Medium" :
                    src.Difficulty == 3 ? "Hard" :
                    "Unknown"));

            // Questions
            CreateMap<Question, QuestionServiceDto>()
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<AnswerOption, AnswerOptionServiceDto>();

            // Quiz Results
            CreateMap<QuizResult, QuizResultResponseServiceDto>()
                .ForMember(dest => dest.QuizId, opt => opt.MapFrom(src => src.QuizId))
                .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.Quiz.Questions.Count))
                .ForMember(dest => dest.CorrectAnswers, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.ScorePercentage, opt => opt.MapFrom(src => src.Percentage))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.QuestionResults, opt => opt.Ignore());

            // Categories
            CreateMap<Category, CategoryServiceDto>();
            CreateMap<CategoryCreateServiceDto, Category>();

            CreateMap<QuestionCreateServiceDto, Question>()
    .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapQuestionType(src.QuestionType)));


        }
        private static QuestionType MapQuestionType(string type) => type switch
        {
            "SingleChoice" => QuestionType.SingleChoice,
            "MultipleChoice" => QuestionType.MultipleChoice,
            "TrueFalse" => QuestionType.TrueFalse,
            "FillInTheBlank" => QuestionType.FillInTheBlank,
            _ => QuestionType.SingleChoice
        };
    }
}
