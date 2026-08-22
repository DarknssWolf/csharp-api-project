using CSharpAPIProject.Application.Users.Commands.ChangeEmailRequest;
using CSharpAPIProject.Application.Users.Commands.ChangePassword;
using CSharpAPIProject.Application.Users.Commands.DeleteUser;
using CSharpAPIProject.Application.Users.Commands.ForgotPassword;
using CSharpAPIProject.Application.Users.Commands.LoginUser;
using CSharpAPIProject.Application.Users.Commands.RefreshAccessToken;
using CSharpAPIProject.Application.Users.Commands.RegisterUser;
using CSharpAPIProject.Application.Users.Commands.ResetPassword;
using CSharpAPIProject.Application.Users.Commands.UpdateUser;
using CSharpAPIProject.Application.Users.Commands.UpdateUserStatus;
using CSharpAPIProject.Application.Users.Models;
using CSharpAPIProject.Application.Users.Queries.GetPaginatedUserList;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class Users : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.MapPost(Login, "login")
				.MapPost(RefreshToken, "refreshToken")
				.MapPost(ForgotPassword, "forgotPassword")
				.MapPost(ResetPassword, "resetPassword")
			;
			app.MapGroup(this)
				.RequireAuthorization()

				.MapPost(ChangePassword, "changePassword")
				.MapPost(ChangeEmailRequest, "changeEmailReq")
				.MapPost(ChangeEmail, "changeEmail")

				.MapPost(RegisterUser, "RegisterUser")

				.MapGet(GetUsers, "")

				.MapPut(UpdateUser, "{id}")
				.MapPatch(UpdateUserStatus, "{id}/status")

				.MapDelete(DeleteUser, "{id}")
			;
		}


		#region Unauthenticated Post Methods

		public async Task<LoginUserCommandResponse> Login(ISender sender, LoginUserCommand command, ILogger<Users> logger)
		{
			logger.LogInformation($"User login attempt: {command.Email}");
			return await sender.Send(command);
		}

		public async Task<RefreshAccessTokenCommandResponse> RefreshToken(ISender sender, RefreshAccessTokenCommand command)
		{
			return await sender.Send(command);
		}

		public async Task<IResult> ForgotPassword(ISender sender, ForgotPasswordCommand command)
		{
			await sender.Send(command);
			return Results.Ok();
		}

		public async Task<IResult> ResetPassword(ISender sender, ResetPasswordCommand command)
		{
			await sender.Send(command);
			return Results.Ok();
		}

		#endregion Unauthenticated Post Methods

		#region Authenticated Post Methods

		public async Task<IResult> ChangePassword(ISender sender, ChangePasswordCommand command)
		{
			await sender.Send(command);
			return Results.Ok();
		}

		public async Task<ChangeEmailRequestCommandResponse> ChangeEmailRequest(ISender sender, ChangeEmailRequestCommand command)
		{
			return await sender.Send(command);
		}

		public async Task<ChangeEmailCommandResponse> ChangeEmail(ISender sender, ChangeEmailCommand command)
		{
			return await sender.Send(command);
		}

		public async Task<IResult> RegisterUser(ISender sender, RegisterUserCommand command)
		{
			await sender.Send(command);
			return Results.Ok();
		}

		#endregion Authenticated Post Methods

		#region Authenticated Get Methods

		public async Task<PaginatedList<UserForVendorDto>> GetUsers(ISender sender, int pageNumber, int pageSize, string? queryFilter = null, int vendorId = -1)
		{
			return await sender.Send(new GetPaginatedUserListQuery()
			{
				PageNumber = pageNumber,
				PageSize = pageSize,
				QueryFilter = queryFilter,
				VendorId = vendorId
			});
		}

		#endregion Authenticated Get Methods

		#region Authenticated Put/Patch Methods

		public async Task<IResult> UpdateUser(ISender sender, Guid id, UpdateUserCommand command)
		{
			command.UserId = id;
			await sender.Send(command);
			return Results.Ok();
		}

		public async Task<IResult> UpdateUserStatus(ISender sender, Guid id, UpdateUserStatusCommand command)
		{
			command.UserId = id;
			await sender.Send(command);
			return Results.Ok();
		}

		#endregion Authenticated Put/Patch Methods

		#region Authenticated Delete Methods

		public async Task<IResult> DeleteUser(ISender sender, Guid id)
		{
			await sender.Send(new DeleteUserCommand(id));
			return Results.Ok();
		}

		#endregion Authenticated Delete Methods
	}
}
