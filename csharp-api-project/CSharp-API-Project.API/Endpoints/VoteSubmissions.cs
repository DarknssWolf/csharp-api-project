using CSharpAPIProject.Application.VoteSubmissions.Commands.DeleteVoteSubmission;
using CSharpAPIProject.Application.VoteSubmissions.Commands.UpsertVoteSubmission;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class VoteSubmissions : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()

				.MapPost(UpsertVoteSubmission, "Upsert")

				.MapDelete(DeleteVoteSubmission, "Delete")
			;
		}

		

		#region Post Methods

		public async Task<IResult> DeleteVoteSubmission(ISender sender, int voteItemId)
		{
			await sender.Send(new DeleteVoteSubmissionCommand() { VoteItemId = voteItemId });
			return Results.Ok();
		}

		public async Task<IResult> UpsertVoteSubmission(ISender sender, UpsertVoteSubmissionCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}


		#endregion Post Methods
	}
}
