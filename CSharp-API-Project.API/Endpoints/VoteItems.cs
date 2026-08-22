using CSharpAPIProject.Application.VoteItems.Commands.DeleteVoteItem;
using CSharpAPIProject.Application.VoteItems.Commands.UpsertVoteItem;
using CSharpAPIProject.Application.VoteItems.Models;
using CSharpAPIProject.Application.VoteItems.Queries.GetPaginatedVoteItemList;
using CSharpAPIProject.Application.VoteItems.Queries.GetVoteItem;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class VoteItems : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()

				.MapGet(GetVoteItem, "")
                .MapGet(GetPaginatedVoteItemList, "List")

                .MapPost(UpsertVoteItem, "Upsert")

				.MapDelete(DeleteVoteItem, "{VoteItemId}")
			;
		}

		#region Get Methods 

		public async Task<VoteItemDto> GetVoteItem(ISender sender, int VoteItemId)
		{
			var result = await sender.Send(new GetVoteItemQuery(VoteItemId));

			return result;
		}

        public async Task<PaginatedList<VoteItemDto>> GetPaginatedVoteItemList(ISender sender, int PageNumber, int PageSize, string? QueryFilter, DateTime? FromDate, DateTime? ToDate)
        {
            var result = await sender.Send(new GetPaginatedVoteItemListQuery(PageNumber, PageSize, QueryFilter, FromDate, ToDate));

            return result;
        }

        #endregion Get Methods

        #region Post Methods

        public async Task<IResult> DeleteVoteItem(ISender sender, int VoteItemId)
		{
			await sender.Send(new DeleteVoteItemCommand() { VoteItemId = VoteItemId });
			return Results.Ok();
		}

		public async Task<IResult> UpsertVoteItem(ISender sender, UpsertVoteItemCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}


		#endregion Post Methods
	}
}
