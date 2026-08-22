using CSharpAPIProject.Application.Users.Commands.AccountRecovery;
using CSharpAPIProject.Application.Users.Commands.ChangeEmailRequest;
using CSharpAPIProject.Application.Users.Commands.ChangePassword;
using CSharpAPIProject.Application.Users.Commands.DeleteUser;
using CSharpAPIProject.Application.Users.Commands.ForgotPassword;
using CSharpAPIProject.Application.Users.Commands.LoginUser;
using CSharpAPIProject.Application.Users.Commands.RefreshAccessToken;
using CSharpAPIProject.Application.Users.Commands.RegisterClientUser;
using CSharpAPIProject.Application.Users.Commands.RegisterTICUser;
using CSharpAPIProject.Application.Users.Commands.ResetPassword;
using CSharpAPIProject.Application.Users.Commands.ResetPasswordOTP;
using CSharpAPIProject.Application.Users.Commands.SendOtp;
using CSharpAPIProject.Application.Users.Commands.UpdateUserDetails;
using CSharpAPIProject.Application.Users.Commands.UpdateUserStatus;
using CSharpAPIProject.Application.Users.Commands.VerifyTotpCode;
using CSharpAPIProject.Application.Users.Models;
using CSharpAPIProject.Application.Users.Queries.GetInfo;
using CSharpAPIProject.Application.Users.Queries.GetRoles;
using CSharpAPIProject.Application.Users.Queries.GetTotpQrCode;
using CSharpAPIProject.Application.Users.Queries.GetUsersForVendor;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class Users : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.MapPost(Login, "login")
				
				.MapGet(SendOtp, "sendOtp")
				
				.MapPost(ForgotPassword, "forgotPassword")
				.MapPost(RefreshToken, "refreshToken")
				.MapPost(ResetPassword, "resetPassword")
				.MapPost(ResetPasswordOTP, "resetPasswordOTP")
				.MapPost(RecoverAccount, "recoverAccount")
			; 
			app.MapGroup(this)
				.RequireAuthorization()
				
				.MapPost(ChangePassword, "changePassword")
				.MapPost(ChangeEmailRequest, "changeEmailReq")
				.MapPost(ChangeEmailCommand, "changeEmail")
				
				.MapPost(RegisterTICUser, "RegisterTICUser")
				.MapPost(RegisterClientUser, "RegisterClientUser")

				.MapGet(GetRoles, "getRoles")
				.MapGet(GetInfo, "getInfo")
				.MapGet(GetTotpQrCode, "getTotpQrCode")
				.MapPost(VerifyTotpCode, "verifyTotpCode")

				.MapGet(GetUserForVendor, "GetUserForVendor")
				.MapPost(UpdateUserStatus, "UpdateUserStatus")
				.MapPost(UpdateUserDetails, "UpdateUserDetails")
				.MapDelete(DeleteUser, "{UserId}")
			;
		}


		#region Unauthenticated Post Methods

		public async Task<LoginUserCommandResponse> Login(ISender sender, LoginUserCommand command, ILogger<Users> logger)
		{
			logger.LogInformation($"User login attempt: {command.Email}");
			return await sender.Send(command);
		}

		public async Task<IResult> ForgotPassword(ISender sender, ForgotPasswordCommand command)
		{
			await sender.Send(command);
			return Results.Ok();
		}

		public async Task<RefreshAccessTokenCommandResponse> RefreshToken(ISender sender, RefreshAccessTokenCommand command)
		{
			return await sender.Send(command);
		}

		public async Task<IResult> ResetPassword(ISender sender, ResetPasswordCommand command)
		{
			await sender.Send(command);
			return TypedResults.Ok();
		}

		public async Task<IResult> ResetPasswordOTP(ISender sender, ResetPasswordOTPCommand command)
		{
			await sender.Send(command);
			return TypedResults.Ok();
		}

		public async Task<AccountRecoveryCommandResponse> RecoverAccount(ISender sender, AccountRecoveryCommand command)
		{
			return await sender.Send(command);
		}

		#endregion Unauthenticated Post Methods

		#region Unauthenticated Get Methods
		public async Task<IResult> SendOtp(
			ISender sender,
			string? email = null,
			string hash = "",
			string? cellphone = null,
			Guid? otpSmsTemplateId = null
		)
		{
			await sender.Send(new SendOtpCommand
			{
				Email = email,
				Hash = hash,
				Cellphone = cellphone,
				OTPSmsTemplateId = otpSmsTemplateId ?? Guid.Empty
			});
			return Results.Ok();
		}

		#endregion Unauthenticated Get Methods


		#region Authenticated Post Methods

		public async Task<IResult> ChangePassword(ISender sender, ChangePasswordCommand command)
		{
			await sender.Send(command);
			return TypedResults.Ok();
		}
		
		public async Task<ChangeEmailRequestCommandResponse> ChangeEmailRequest(ISender sender, ChangeEmailRequestCommand command)
		{
			return await sender.Send(command);
		}
		
		public async Task<ChangeEmailCommandResponse> ChangeEmailCommand(ISender sender, ChangeEmailCommand command)
		{
			return await sender.Send(command);
		}
		
		public async Task<IResult> RegisterTICUser(ISender sender, RegisterTICUserCommand command)
		{
			await sender.Send(command);
			return TypedResults.Ok();
		}
		
		public async Task<IResult> RegisterClientUser(ISender sender, RegisterClientUserCommand command)
		{
			await sender.Send(command);
			return TypedResults.Ok();
		}

        public async Task<IResult> UpdateUserStatus(ISender sender, UpdateUserStatusCommand command)
        {
            await sender.Send(command);
            return TypedResults.Ok();
        }

        public async Task<IResult> UpdateUserDetails(ISender sender, UpdateUserCommand command)
        {
            await sender.Send(command);
            return TypedResults.Ok();
        }

        #endregion Authenticated Post Methods

        #region Authenticated Get Methods

        public async Task<GetRolesResponse> GetRoles(ISender sender)
		{
			return await sender.Send(new GetRolesQuery());
		}

		public async Task<GetInfoResponse> GetInfo(ISender sender)
		{
			return await sender.Send(new GetInfoRequest());
		}

		public async Task<GetTotpQrCodeResponse> GetTotpQrCode(ISender sender)
		{
			return await sender.Send(new GetTotpQrCodeQuery());
		}

		public async Task<VerifyTotpCodeResponse> VerifyTotpCode(ISender sender, VerifyTotpCodeCommand command)
		{
			return await sender.Send(command);
		}

        public async Task<PaginatedList<UserForVendorDto>> GetUserForVendor(
            ISender sender,
            int? vendorId = 0,
            int pageNumber = 1,
            int pageSize = 10,
            string queryFilter = "",
            string roleName = "",
            Guid? designationId = null)
        {
            return await sender.Send(new GetUsersForVendorQuery()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                QueryFilter = queryFilter,
                VendorId = vendorId ?? 0,
                Role = roleName,
                DesignationId = designationId
            });
        }

        #endregion Authenticated Get Methods

        #region Authenticated Delete Methods
        public async Task<IResult> DeleteUser(ISender sender, string userId)
        {
            await sender.Send(new DeleteUserCommand() { UserId = userId });
            return Results.Ok();
        }
        #endregion Authenticated Delete Methods
    }
}
