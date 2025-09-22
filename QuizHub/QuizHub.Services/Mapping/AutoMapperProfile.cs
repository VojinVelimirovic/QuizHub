using AutoMapper;
using QuizHub.Data.Models;
using QuizHub.Services.DTOs.Users;
//using QuizHub.Services.DTOs.Quizzes;
//using QuizHub.Services.DTOs.Questions;
//using QuizHub.Services.DTOs.QuizResults;

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
            //CreateMap<QuizCreateDto, Quiz>();
            //CreateMap<Quiz, QuizResultDto>();
            //CreateMap<Quiz, QuizDetailDto>();

            // Questions
            //CreateMap<QuestionCreateDto, Question>();
            //CreateMap<Question, QuestionDto>();
            //CreateMap<AnswerOptionCreateDto, AnswerOption>();
            //CreateMap<AnswerOption, AnswerOptionDto>();

            // Quiz Results
            //CreateMap<QuizResult, QuizResultDto>();
        }
    }
}
