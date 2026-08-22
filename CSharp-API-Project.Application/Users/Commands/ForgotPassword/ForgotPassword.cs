using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Commands.ForgotPassword
{
	public class ForgotPasswordCommand : IRequest
	{
		public string Email { get; set; }
	}


	public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
	{
		public ForgotPasswordValidator()
		{
			RuleFor(v => v.Email).NotEmpty().EmailAddress();
		}
	}

	public class ForgotPasswordCommandHandler(
		IIdentityService identityService
	) : IRequestHandler<ForgotPasswordCommand>
	{

		public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
		{
			var user = await identityService.IsUser(request.Email);
			if (!user)
			{
				return;
			}
			var userId = await identityService.GetUserIdAsync(request.Email);
            var userClaims = await identityService.GetClaimsAsync(userId);
            var userStatus = userClaims.FirstOrDefault(x => x.Type == "UserStatus")?.Value;
            if (userStatus == UserStatus.Deleted.ToString())
            {
                throw new UserInDeleteStatusException();
            }

			// The recovery code gets mailed to the user and is consumed by the ResetPassword command.
			var recoveryCode = await identityService.GeneratePasswordResetToken(userId);

			// send emails here
		}
	}
}
