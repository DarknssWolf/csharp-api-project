using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Files.Helpers;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

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
	ITenantResolver tenantResolver) : IRequestHandler<GetPaginatedSurveyListQuery, PaginatedList<SurveyDto>>
{
	public async Task<PaginatedList<SurveyDto>> Handle(GetPaginatedSurveyListQuery request, CancellationToken cancellationToken)
	{
		var vendorId = tenantResolver.ResolveTenantId();

		var query = context.Surveys
            .Include(s => s.SurveyQuestions)
            .Where(s => s.VendorId == vendorId && s.StatusFlag.Status == RecordStatus.Active);

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
			surveyDtos[i].SurveyData = FileDataHelper.AddFileUri(surveyDtos[i].SurveyData);
			foreach (var question in surveyDtos[i].Questions)
				question.SurveyQuestionData = FileDataHelper.AddFileUri(question.SurveyQuestionData);

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
