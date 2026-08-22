using CSharpAPIProject.Application.SurveyResponseSets.Models;

namespace CSharpAPIProject.Application.SurveyResponseSets.Mappings;

public class SurveyResponseSetDtoMappings : Profile
{
	public SurveyResponseSetDtoMappings()
	{
		CreateMap<Domain.Entities.SurveyResponseSet, SurveyResponseSetDto>();
	}
}
