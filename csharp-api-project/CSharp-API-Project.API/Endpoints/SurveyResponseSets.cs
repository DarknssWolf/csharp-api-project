using CSharpAPIProject.Application.SurveyResponseSets.Commands.DeleteSurveyResponseSet;
using CSharpAPIProject.Application.SurveyResponseSets.Commands.UpsertSurveyResponseSet;
using CSharpAPIProject.Application.SurveyResponseSets.Models;
using CSharpAPIProject.Application.SurveyResponseSets.Queries.GetPaginatedSurveyResponseSetList;
using CSharpAPIProject.Application.SurveyResponseSets.Queries.GetSurveyResponseSet;
using CSharpAPIProject.Application.SurveyResponseSets.Queries.GetSurveyResponseSetByCustomerAndSurveyId;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class SurveyResponseSets : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()
				.MapGet(GetSurveyResponseSet, "")
				.MapGet(GetSurveyResponseSetByCustomerAndSurveyId, "ByCustomerAndSurvey/{customerId}/{surveyId}")
				.MapGet(GetPaginatedSurveyResponseSetList, "List")
				.MapPost(UpsertSurveyResponseSet, "Upsert")
				.MapDelete(DeleteSurveyResponseSet, "{SurveyResponseSetId}")
			;
		}

		#region Get Methods 

		public async Task<SurveyResponseSetDto> GetSurveyResponseSet(ISender sender, int SurveyResponseSetId)
		{
			var result = await sender.Send(new GetSurveyResponseSetQuery(SurveyResponseSetId));

			return result;
		}

		public async Task<SurveyResponseSetDto> GetSurveyResponseSetByCustomerAndSurveyId(ISender sender, int customerId, int surveyId)
		{
			var result = await sender.Send(new GetSurveyResponseSetByCustomerAndSurveyIdQuery(customerId, surveyId));

			return result;
		}

		public async Task<PaginatedList<SurveyResponseSetDto>> GetPaginatedSurveyResponseSetList(
			ISender sender, 
			int pageNumber = 1, 
			int pageSize = 10, 
			string queryFilter = "",
			int? surveyId = null,
			int? customerId = null)
		{
			return await sender.Send(new GetPaginatedSurveyResponseSetListQuery(pageNumber, pageSize, queryFilter, surveyId, customerId));
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> UpsertSurveyResponseSet(ISender sender, UpsertSurveyResponseSetCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}

		#endregion Post Methods

		#region Delete Methods

		public async Task<IResult> DeleteSurveyResponseSet(ISender sender, int SurveyResponseSetId)
		{
			await sender.Send(new DeleteSurveyResponseSetCommand(SurveyResponseSetId));
			return Results.Ok();
		}

		#endregion Delete Methods
	}
}
