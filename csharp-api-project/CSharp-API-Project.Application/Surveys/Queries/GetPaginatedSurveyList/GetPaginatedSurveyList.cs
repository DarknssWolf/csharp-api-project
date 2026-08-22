using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Surveys.Helpers;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Text.Json;
using CSharpAPIProject.Application.Common.Interfaces.Services;

namespace CSharpAPIProject.Application.Surveys.Queries.GetPaginatedSurveyList;

[Authorize(Policy = Policies.CanView_Survey)]
public record GetPaginatedSurveyListQuery(
	int PageNumber,
	int PageSize,
	string QueryFilter,
    RecordStatus RecordStatusFilter
) : IRequest<PaginatedList<SurveyDto>>;

public class GetPaginatedSurveyListQueryValidator : AbstractValidator<GetPaginatedSurveyListQuery>
{
	public GetPaginatedSurveyListQueryValidator()
	{
		RuleFor(x => x.PageNumber).NotEmpty();
		RuleFor(x => x.PageSize).NotEmpty().LessThan(1000);
	}
}

public class GetPaginatedSurveyListQueryHandler(
	IApplicationDbContext context, 
	IMapper mapper,
	IConfiguration configuration, 
	ITenantResolver tenantResolver,
	ISurveyResponseHelperService surveyResponseHelperService,
	IVisibilityService visibilityService,
	IUser user) : IRequestHandler<GetPaginatedSurveyListQuery, PaginatedList<SurveyDto>>
{
	public async Task<PaginatedList<SurveyDto>> Handle(GetPaginatedSurveyListQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();
        var visibleConfigIds = await visibilityService.GetVisibleConfigIdsAsync(Guid.Parse(user.Id), vendorId);
        
		var query = context.Surveys
            .Include(s => s.SurveyQuestions)
            .Where(s => s.VendorId == vendorId && s.StatusFlag.Status == RecordStatus.Active)
            .Where(vs => vs.VisibilityConfigurationId != null && visibleConfigIds.Contains(vs.VisibilityConfigurationId.Value));

		if (string.IsNullOrWhiteSpace(request.QueryFilter) == false)
		{
			query = query
				.Where(s => s.Title.Contains(request.QueryFilter) || s.Code.Contains(request.QueryFilter));
		}

		//convert to list
		var surveys = await query.ToListAsync();

		//Map to DTOs
		var surveyDtos = surveys.Select(s => mapper.Map<SurveyDto>(s)).ToList();

		for (int i = 0; i < surveyDtos.Count; i++)
		{
			surveyDtos[i] = await SurveyHelper.GetGalleryImagesForSurvey(configuration, context, surveyDtos[i], cancellationToken);
			surveyDtos[i].Questions = surveyDtos[i].Questions.OrderBy(q => q.QuestionId).ToList();
		}



		//Paginate
		var paginatedResult = surveyDtos
			.Skip((request.PageNumber - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToList();

		//Return results
		return new PaginatedList<SurveyDto>(paginatedResult, surveyDtos.Count(), request.PageNumber, request.PageSize);
	}
}
