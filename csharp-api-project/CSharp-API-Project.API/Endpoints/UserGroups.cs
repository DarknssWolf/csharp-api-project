using CSharpAPIProject.Application.UserGroups.Commands.DeleteUserGroup;
using CSharpAPIProject.Application.UserGroups.Commands.UpsertUserGroup;
using CSharpAPIProject.Application.UserGroups.Models;
using CSharpAPIProject.Application.UserGroups.Queries.GetUserGroup;
using CSharpAPIProject.Application.UserSubGroups.Commands.DeleteUserSubGroup;
using CSharpAPIProject.Application.UserGroups.Queries.GetPaginatedUserGroupList;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class UserGroups : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()

				.MapGet(GetUserGroup, "")
				.MapGet(GetPaginatedUserGroupList, "List")

				.MapPost(UpsertUserGroup, "Upsert")

				.MapDelete(DeleteUserGroup, "{UserGroupId}")
				//.MapDelete(DeleteUserSubGroup, "{UserSubGroupId}")
			;
		}

		#region Get Methods 

		public async Task<UserGroupDto> GetUserGroup(ISender sender, Guid UserGroupId)
		{
			var result = await sender.Send(new GetUserGroupQuery(UserGroupId));

			return result;
		}

		public async Task<PaginatedList<UserGroupDto>> GetPaginatedUserGroupList(ISender sender, int pageNumber = 1, int pageSize = 10, string queryFilter = "")
		{
			return await sender.Send(new GetPaginatedUserGroupListQuery(pageNumber, pageSize, queryFilter));
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> DeleteUserGroup(ISender sender, Guid UserGroupId)
		{
			await sender.Send(new DeleteUserGroupCommand() { UserGroupId = UserGroupId });
			return Results.Ok();
		}

        //public async Task<IResult> DeleteUserSubGroup(ISender sender, Guid UserSubGroupId)
        //{
        //    await sender.Send(new DeleteUserSubGroupCommand() { UserSubGroupId = UserSubGroupId });
        //    return Results.Ok();
        //}

        public async Task<IResult> UpsertUserGroup(ISender sender, UpsertUserGroupCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}


		#endregion Post Methods
	}
}
