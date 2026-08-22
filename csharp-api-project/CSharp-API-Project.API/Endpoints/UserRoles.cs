using CSharpAPIProject.Application.UserRoles.Commands.GiveRole;
using CSharpAPIProject.Application.UserRoles.Commands.TakeRole;
using CSharpAPIProject.Application.UserRoles.Queries.GetPromotionPaths;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints;

public class UserRoles : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		app.MapGroup(this)
			.RequireAuthorization()

			.MapGet(GetPromotionPaths, "GetPromotionPaths")

			.MapPost(GiveRole, "GiveRole")
			.MapPost(TakeRole, "TakeRole")
		;
	}

	#region Authenticated Get Methods

	public async Task<IList<string?>> GetPromotionPaths(ISender sender)
	{
		return await sender.Send(new GetPromotionPathQuery());
	}

	#endregion Authenticated Get Methods

	#region Authenticated Post Methods

	public async Task<IResult> GiveRole(ISender sender, GiveRoleCommand command)
	{
		await sender.Send(command);
		return TypedResults.Ok();
	}
	
	public async Task<IResult> TakeRole(ISender sender, TakeRoleCommand command)
	{
		await sender.Send(command);
		return TypedResults.Ok();
	}

	#endregion Authenticated Post Methods
}
