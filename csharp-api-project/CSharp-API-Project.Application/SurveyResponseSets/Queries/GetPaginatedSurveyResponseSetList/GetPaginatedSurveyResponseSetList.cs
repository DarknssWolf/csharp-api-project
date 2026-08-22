using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponseSets.Models;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Queries.GetPaginatedSurveyResponseSetList;

[Authorize(Policy = Policies.CanView_SurveyResponseSet)]
public record GetPaginatedSurveyResponseSetListQuery(
	int PageNumber,
	int PageSize,
	string QueryFilter,
	int? SurveyId,
	int? CustomerId
) : IRequest<PaginatedList<SurveyResponseSetDto>>;

public class GetPaginatedSurveyResponseSetListQueryValidator : AbstractValidator<GetPaginatedSurveyResponseSetListQuery>
{
	public GetPaginatedSurveyResponseSetListQueryValidator()
	{
		RuleFor(x => x.PageNumber).NotEmpty();
		RuleFor(x => x.PageSize).NotEmpty().LessThan(1000);
	}
}

public class GetPaginatedSurveyResponseSetListQueryHandler(
	IApplicationDbContext context,
	IMapper mapper,
	ITenantResolver tenantResolver) : IRequestHandler<GetPaginatedSurveyResponseSetListQuery, PaginatedList<SurveyResponseSetDto>>
{
	public async Task<PaginatedList<SurveyResponseSetDto>> Handle(GetPaginatedSurveyResponseSetListQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
		var query = context.SurveyResponseSets
			.Where(srs => srs.VendorId == vendorId);

		// Filter by SurveyId if provided
		if (request.SurveyId.HasValue)
		{
			query = query.Where(srs => srs.SurveyId == request.SurveyId.Value);
		}

		// Filter by CustomerId if provided
		if (request.CustomerId.HasValue)
		{
			query = query.Where(srs => srs.CustomerId == request.CustomerId.Value);
		}

		// Apply text filter if provided
		if (!string.IsNullOrWhiteSpace(request.QueryFilter))
		{
			query = query.Where(srs => 
				srs.Survey.Title.Contains(request.QueryFilter));
		}

		// Include related entities for mapping
		query = query
			.Include(srs => srs.Survey)
			.Include(srs => srs.Customer)
            .Where(x => x.StatusFlag.Status == RecordStatus.Active)
            .OrderByDescending(srs => srs.StartDate);

		var surveyResponseSets = await query
			.Skip((request.PageNumber - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		var totalCount = await query.CountAsync(cancellationToken);

		var surveyResponseSetDtos = mapper.Map<List<SurveyResponseSetDto>>(surveyResponseSets);

		return new PaginatedList<SurveyResponseSetDto>(surveyResponseSetDtos, totalCount, request.PageNumber, request.PageSize);
	}
}
