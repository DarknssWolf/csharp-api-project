using CSharpAPIProject.Application.SurveyResponses.Commands.UpsertSurveyResponse;
using CSharpAPIProject.Application.SurveyResponses.Models;
using CSharpAPIProject.Application.SurveyResponses.Queries.GetPaginatedSurveyResponseList;
using CSharpAPIProject.Application.SurveyResponses.Queries.GetSurveyResponse;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class SurveyResponses : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()
				.MapGet(GetSurveyResponse, "")
				.MapGet(GetPaginatedSurveyResponseList, "List")
				.MapPost(UpsertSurveyResponse, "Upsert")
			;
		}

		#region Get Methods 

		public async Task<SurveyResponseDto> GetSurveyResponse(ISender sender, int surveyId, int surveyQuestionId, int customerId)
		{
			return await sender.Send(new GetSurveyResponseQuery()
			{
				SurveyId = surveyId,
				SurveyQuestionId = surveyQuestionId,
				CustomerId = customerId
			});
		}

		public async Task<PaginatedList<SurveyResponseDto>> GetPaginatedSurveyResponseList(ISender sender, int surveyId, int pageNumber = 1, int pageSize = 10)
		{
			return await sender.Send(new GetPaginatedSurveyResponseListQuery()
			{
				PageNumber = pageNumber,
				PageSize = pageSize,
				SurveyId = surveyId
			});
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> UpsertSurveyResponse(ISender sender, UpsertSurveyResponseCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}
		#endregion Post Methods
	}
}
