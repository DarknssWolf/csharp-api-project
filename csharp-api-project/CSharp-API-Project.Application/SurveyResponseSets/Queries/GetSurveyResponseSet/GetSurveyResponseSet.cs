using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponseSets.Models;
using CSharpAPIProject.Application.SurveyResponseSets.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Queries.GetSurveyResponseSet;

[Authorize(Policy = Policies.CanView_SurveyResponseSet)]
public record GetSurveyResponseSetQuery(int SurveyResponseSetId) : IRequest<SurveyResponseSetDto>;

public class GetSurveyResponseSetQueryValidator : AbstractValidator<GetSurveyResponseSetQuery>
{
	public GetSurveyResponseSetQueryValidator()
	{
		RuleFor(x => x.SurveyResponseSetId).GreaterThan(0);
	}
}

public class GetSurveyResponseSetQueryHandler(
	IApplicationDbContext context,
	IMapper mapper,
	ITenantResolver tenantResolver) : IRequestHandler<GetSurveyResponseSetQuery, SurveyResponseSetDto>
{
	public async Task<SurveyResponseSetDto> Handle(GetSurveyResponseSetQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

		var surveyResponseSet = await context.SurveyResponseSets
            .Where(srs => srs.StatusFlag.Status == RecordStatus.Active)
            .FirstOrDefaultAsync(srs => srs.SurveyResponseSetId == request.SurveyResponseSetId && srs.VendorId == vendorId, cancellationToken);

		if (surveyResponseSet == null)
			throw new SurveyResponseSetNotFoundException(request.SurveyResponseSetId);

		var surveyResponseSetDto = mapper.Map<SurveyResponseSetDto>(surveyResponseSet);

		return surveyResponseSetDto;
	}
}
