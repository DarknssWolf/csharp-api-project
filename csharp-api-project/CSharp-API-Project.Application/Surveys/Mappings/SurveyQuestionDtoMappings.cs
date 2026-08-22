using CSharpAPIProject.Application.Surveys.Models;

namespace CSharpAPIProject.Application.Surveys.Mappings;

public class SurveyQuestionDtoMappings : Profile
{
	public SurveyQuestionDtoMappings()
	{
		CreateMap<Domain.Entities.SurveyQuestion, SurveyQuestionDto>()
			.ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.QuestionId))
			.ForMember(dest => dest.ParentQuestionId, opt => opt.MapFrom(src => src.ParentQuestionId))
			.ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt))
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
			.ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
			.ForMember(dest => dest.ExpectedAnswers, opt => opt.MapFrom(src => src.ExpectedAnswers))
			.ForMember(dest => dest.AllowMultipleResponses, opt => opt.MapFrom(src => src.AllowMultipleResponses))
			.ForMember(dest => dest.ParentCondition, opt => opt.MapFrom(src => src.ParentCondition))
			.ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber))
			.ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.SurveyId))
			.ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => src.IsRequired))
			.ForMember(dest => dest.SurveyQuestionData, opt => opt.MapFrom(src => src.SurveyQuestionData))
			;

		// Add the reverse mapping
		CreateMap<SurveyQuestionDto, Domain.Entities.SurveyQuestion>()
			.ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.QuestionId))
			.ForMember(dest => dest.ParentQuestionId, opt => opt.MapFrom(src => src.ParentQuestionId))
			.ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt))
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
			.ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
			.ForMember(dest => dest.ExpectedAnswers, opt => opt.MapFrom(src => src.ExpectedAnswers))
			.ForMember(dest => dest.AllowMultipleResponses, opt => opt.MapFrom(src => src.AllowMultipleResponses))
			.ForMember(dest => dest.ParentCondition, opt => opt.MapFrom(src => src.ParentCondition))
			.ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber))
			.ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.SurveyId))
			.ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => src.IsRequired))
			.ForMember(dest => dest.SurveyQuestionData, opt => opt.MapFrom(src => src.SurveyQuestionData));
	}
}
