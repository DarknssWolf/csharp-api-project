using CSharpAPIProject.Application.SurveyResponses.Models;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.SurveyResponses.Mappings
{
	public class SurveyResponseDtoMappings : Profile
	{
		public SurveyResponseDtoMappings()
		{
			// Map individual SurveyResponse to SurveyQuestionResponseDto
			CreateMap<SurveyResponse, SurveyQuestionResponseDto>()
				.ForMember(x => x.SurveyQuestionId, y => y.MapFrom(s => s.SurveyQuestionId))
				.ForMember(x => x.Response, y => y.MapFrom(s => s.Response));

			// Map a collection of SurveyResponse to SurveyResponseDto
			CreateMap<List<SurveyResponse>, SurveyResponseDto>()
				.ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.First().VendorId))
				.ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.First().SurveyId))
				.ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.First().CustomerId))
				.ForMember(dest => dest.SurveyResponseSetId, opt => opt.MapFrom(src => src.First().SurveyResponseSetId))
				.ForMember(dest => dest.Responses, opt => opt.MapFrom(src => src));
		}
	}
}
