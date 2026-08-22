using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Helpers;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using CSharpAPIProject.Application.Common.Interfaces.Services;

namespace CSharpAPIProject.Application.Surveys.Queries.GetSurvey;

[Authorize(Policy = Policies.CanView_Survey)]
public record GetSurveyQuery(
	int SurveyId,
    RecordStatus recordStatusFilter
) : IRequest<SurveyDto>;

public class GetSurveyQueryValidator : AbstractValidator<GetSurveyQuery>
{
	public GetSurveyQueryValidator()
	{
		RuleFor(x => x.SurveyId).GreaterThan(0);
	}
}

public class GetSurveyQueryHandler(
	IApplicationDbContext context,
	IConfiguration configuration, 
	IMapper mapper, 
	ITenantResolver tenantResolver,
	ISurveyResponseHelperService surveyResponseHelperService,
	IVisibilityService visibilityService,
	IUser user) : IRequestHandler<GetSurveyQuery, SurveyDto>
{
	public async Task<SurveyDto> Handle(GetSurveyQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
        
        var visibleConfigIds = await visibilityService.GetVisibleConfigIdsAsync(Guid.Parse(user.Id), vendorId);

        var survey = await context.Surveys
            .Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
            .Where(s => s.SurveyId == request.SurveyId)
            .Where(s => s.VendorId == vendorId)
            .Where(s => s.StatusFlag.Status == RecordStatus.Active)
            .Where(vs => vs.VisibilityConfigurationId != null && visibleConfigIds.Contains(vs.VisibilityConfigurationId.Value))
            .FirstOrDefaultAsync(cancellationToken);

		// Check if the Survey exists
		if (survey == null)
		{
			throw new SurveyWithVendorAndSurveyIdNotFoundException(request.SurveyId, vendorId);
		}
		
		//Map to DTO
		var surveyDto = mapper.Map<SurveyDto>(survey);

		// Ensure questions are ordered by QuestionId for editing scenarios
		surveyDto.Questions = surveyDto.Questions
			.OrderBy(q => q.OrderNumber)
			.ToList();

		//update question data to get galleryUri
		surveyDto = await SurveyHelper.GetGalleryImagesForSurvey(configuration, context, surveyDto, cancellationToken);

		return surveyDto;
	}

}