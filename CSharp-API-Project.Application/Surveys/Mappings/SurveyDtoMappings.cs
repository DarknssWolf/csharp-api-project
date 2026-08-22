using CSharpAPIProject.Application.Surveys.Models;

namespace CSharpAPIProject.Application.Surveys.Mappings;

public class SurveyDtoMappings : Profile
{
	public SurveyDtoMappings()
	{
		CreateMap<Domain.Entities.Survey, SurveyDto>()
			.ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.SurveyId))
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
			.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
			.ForMember(dest => dest.StartOn, opt => opt.MapFrom(src => src.StartOn))
			.ForMember(dest => dest.EndOn, opt => opt.MapFrom(src => src.EndOn))
			.ForMember(dest => dest.CanEarnPoints, opt => opt.MapFrom(src => src.CanEarnPoints))
			.ForMember(dest => dest.AllowMultipleTries, opt => opt.MapFrom(src => src.AllowMultipleTries))
			.ForMember(dest => dest.MultipleTriesLimit, opt => opt.MapFrom(src => src.MultipleTriesLimit))
			.ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points))
			.ForMember(dest => dest.SurveyData, opt => opt.MapFrom(src => src.SurveyData))
			.ForMember(dest => dest.VisibilityConfigurationId, opt => opt.MapFrom(src => src.VisibilityConfigurationId))
			.ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.SurveyQuestions ?? new List<Domain.Entities.SurveyQuestion>()))
			.ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.SurveyQuestions != null ? src.SurveyQuestions.Count : 0))
		;
		// Mapping for the SurveyQuestions property
		CreateMap<Domain.Entities.SurveyQuestion, SurveyQuestionDto>();
	}
}
