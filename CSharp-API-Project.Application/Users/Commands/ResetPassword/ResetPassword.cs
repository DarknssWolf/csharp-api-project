using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Enums;
namespace CSharpAPIProject.Application.Users.Commands.ResetPassword
{
	public class ResetPasswordCommand : IRequest
	{
		public string Email { get; set; }
		public string NewPassword { get; set; }
		public string RecoveryCode { get; set; }
	}

	public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
	{
		public ResetPasswordCommandValidator()
		{
			RuleFor(v => v.Email)
				.NotEmpty()
				.EmailAddress()
			;

			RuleFor(v => v.NewPassword).NotEmpty().WithMessage("Password is required.")
				.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");

			RuleFor(v => v.RecoveryCode).NotEmpty();
		}
	}

	public class ResetPasswordCommandHandler(
		IIdentityService identityService
	) : IRequestHandler<ResetPasswordCommand>
	{
		public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
		{
			var userId = await identityService.GetUserIdAsync(request.Email);
			if (userId == null)
				throw new UserNotFoundException(request.Email);

            var userClaims = await identityService.GetClaimsAsync(userId);
            var userStatus = userClaims.FirstOrDefault(x => x.Type == "UserStatus")?.Value;
            if (userStatus == UserStatus.Deleted.ToString())
            {
                throw new UserInDeleteStatusException();
            }

            var result = await identityService.ResetPasswordAsync(request.Email, request.RecoveryCode, request.NewPassword);
			if (!result)
				throw new Exception("Failed to reset password.");

			// send emails here
		}
	}
}
