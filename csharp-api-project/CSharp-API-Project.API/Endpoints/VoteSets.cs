using CSharpAPIProject.Application.VoteSets.Commands.DeleteVoteSet;
using CSharpAPIProject.Application.VoteSets.Commands.UpsertVoteSet;
using CSharpAPIProject.Application.VoteSets.Models;
using CSharpAPIProject.Application.VoteSets.Queries.GetPaginatedVoteSetList;
using CSharpAPIProject.Application.VoteSets.Queries.GetVoteSet;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class VoteSets : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()

				.MapGet(GetVoteSet, "")
				.MapGet(GetPaginatedVoteSetList, "List")

				.MapPost(UpsertVoteSet, "Upsert")

				.MapDelete(DeleteVoteSet, "{VoteSetId}")
			;
		}

		#region Get Methods 

		public async Task<VoteSetDto> GetVoteSet(ISender sender, int VoteSetId)
		{
			var result = await sender.Send(new GetVoteSetQuery(VoteSetId));

			return result;
		}

		public async Task<PaginatedList<VoteSetDto>> GetPaginatedVoteSetList(ISender sender, int pageNumber = 1, int pageSize = 10, string queryFilter = "", bool? archive = false)
		{
			return await sender.Send(new GetPaginatedVoteSetListQuery(pageNumber, pageSize, queryFilter, archive ?? false));
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> DeleteVoteSet(ISender sender, int VoteSetId)
		{
			await sender.Send(new DeleteVoteSetCommand() { VoteSetId = VoteSetId });
			return Results.Ok();
		}

		public async Task<IResult> UpsertVoteSet(ISender sender, UpsertVoteSetCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}


		#endregion Post Methods
	}
}
