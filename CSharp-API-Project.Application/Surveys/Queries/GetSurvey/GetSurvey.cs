using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.Surveys.Exceptions;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

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
	IMapper mapper,
	ITenantResolver tenantResolver) : IRequestHandler<GetSurveyQuery, SurveyDto>
{
	public async Task<SurveyDto> Handle(GetSurveyQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

        var survey = await context.Surveys
            .Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
            .Where(s => s.SurveyId == request.SurveyId)
            .Where(s => s.VendorId == vendorId)
            .Where(s => s.StatusFlag.Status == RecordStatus.Active)
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

		// Add download links for any stored files referenced in the data
		surveyDto.SurveyData = FileDataHelper.AddFileUri(surveyDto.SurveyData);
		foreach (var question in surveyDto.Questions)
			question.SurveyQuestionData = FileDataHelper.AddFileUri(question.SurveyQuestionData);

		return surveyDto;
	}

}
