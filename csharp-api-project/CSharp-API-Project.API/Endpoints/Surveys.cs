using CSharpAPIProject.Application.Surveys.Commands.DeleteSurvey;
using CSharpAPIProject.Application.Surveys.Commands.UpsertSurvey;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Application.Surveys.Queries.GetPaginatedSurveyList;
using CSharpAPIProject.Application.Surveys.Queries.GetSurvey;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class Surveys : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()

				.MapGet(GetSurvey, "")
				.MapGet(GetPaginatedSurveyList, "List")

				//.MapPost(CreateSurvey, "Create")
				.MapPost(UpsertSurvey, "Upsert")

				.MapDelete(DeleteSurvey, "{SurveyId}")
			;
		}

		#region Get Methods 

		public async Task<SurveyDto> GetSurvey(ISender sender, int SurveyId, RecordStatus recordStatusFilter = RecordStatus.Active)
		{
			var result = await sender.Send(new GetSurveyQuery(SurveyId, recordStatusFilter));

			return result;
		}

		public async Task<PaginatedList<SurveyDto>> GetPaginatedSurveyList(ISender sender, int pageNumber = 1, int pageSize = 10, string queryFilter = "", RecordStatus recordStatusFilter = RecordStatus.Active)
		{
			return await sender.Send(new GetPaginatedSurveyListQuery(pageNumber, pageSize, queryFilter, recordStatusFilter));
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> DeleteSurvey(ISender sender, int surveyId)
		{
			await sender.Send(new DeleteSurveyCommand() { SurveyId = surveyId });
			return Results.Ok();
		}

		public async Task<IResult> UpsertSurvey(ISender sender, UpsertSurveyCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}


		#endregion Post Methods
	}
}
