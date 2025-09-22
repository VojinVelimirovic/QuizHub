using AutoMapper;
using QuizHub.Data.Models;
using QuizHub.Api.DTOs.Users;
using QuizHub.Api.DTOs.Quizzes;
using QuizHub.Api.DTOs.Questions;
using QuizHub.Api.DTOs.QuizResults;

namespace QuizHub.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<UserRegisterDto, User>();

            CreateMap<Quiz, QuizResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));

            CreateMap<Quiz, QuizDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<QuizCreateDto, Quiz>();

            CreateMap<Question, QuestionDto>();
            CreateMap<AnswerOption, AnswerOptionDto>();

            CreateMap<QuizResult, QuizResultResponseDto>();
        }
    }
}
