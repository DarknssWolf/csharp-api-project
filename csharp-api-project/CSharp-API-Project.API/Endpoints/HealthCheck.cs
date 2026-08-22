using CSharpAPIProject.Api.Infrastructure;
using MediatR;

namespace CSharpAPIProject.Api.Endpoints
{
	public class HealthCheck : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.MapGet(Return200Ok, "")
			;
		}

		public async Task<IResult> Return200Ok(ISender sender)
		{
			return Results.Ok();
		}
	}

}
