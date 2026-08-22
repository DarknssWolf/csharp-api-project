using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponseSets.Models;
using CSharpAPIProject.Application.SurveyResponseSets.Exceptions;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Queries.GetSurveyResponseSetByCustomerAndSurveyId;

[Authorize(Policy = Policies.CanView_SurveyResponseSet)]
public record GetSurveyResponseSetByCustomerAndSurveyIdQuery(
	int CustomerId,
	int SurveyId
) : IRequest<SurveyResponseSetDto>;

public class GetSurveyResponseSetByCustomerAndSurveyIdQueryValidator : AbstractValidator<GetSurveyResponseSetByCustomerAndSurveyIdQuery>
{
	public GetSurveyResponseSetByCustomerAndSurveyIdQueryValidator()
	{
		RuleFor(x => x.CustomerId).GreaterThan(0);
		RuleFor(x => x.SurveyId).GreaterThan(0);
	}
}

public class GetSurveyResponseSetByCustomerAndSurveyIdQueryHandler(
	IApplicationDbContext context,
	IMapper mapper,
	ITenantResolver tenantResolver) : IRequestHandler<GetSurveyResponseSetByCustomerAndSurveyIdQuery, SurveyResponseSetDto>
{
	public async Task<SurveyResponseSetDto> Handle(GetSurveyResponseSetByCustomerAndSurveyIdQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

        var surveyResponseSet = await context.SurveyResponseSets
            .Where(x => x.StatusFlag.Status == RecordStatus.Active)
            .FirstOrDefaultAsync(srs => srs.VendorId == vendorId && 
                                      srs.CustomerId == request.CustomerId && 
                                      srs.SurveyId == request.SurveyId, cancellationToken);

		// Check if the SurveyResponseSet exists
		if (surveyResponseSet == null)
		{
			throw new SurveyResponseSetWithCustomerAndSurveyNotFoundException(request.CustomerId, request.SurveyId);
		}

		//Map to DTO
		var surveyResponseSetDto = mapper.Map<SurveyResponseSetDto>(surveyResponseSet);

		return surveyResponseSetDto;
	}
}
