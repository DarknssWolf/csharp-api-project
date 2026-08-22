using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Commands.ChangePassword
{
	[Authorize(Roles = Roles.Administrator)]
	[Authorize(Roles = Roles.Vendor)]
	[Authorize(Roles = Roles.Customer)]
	public class ChangePasswordCommand : IRequest
	{
		public string Email { get; set; }
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
	}

	public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
	{
		public ChangePasswordCommandValidator()
		{
			RuleFor(v => v.Email).NotEmpty().EmailAddress();

			RuleFor(v => v.CurrentPassword).NotEmpty();

			RuleFor(v => v.NewPassword).NotEmpty().WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");
		}
	}

	public class ChangePasswordCommandHandler(
		IUser user,
		IIdentityService identityService
	) : IRequestHandler<ChangePasswordCommand>
	{
		public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
		{
			var email = await identityService.GetUserEmailAsync(user.Id);
			if (email != request.Email)
				throw new ChangePasswordEmailMismatchException();

            var userClaims = await identityService.GetClaimsAsync(user.Id);
            var userStatus = userClaims.FirstOrDefault(x => x.Type == "UserStatus")?.Value;
            if (userStatus == UserStatus.Deleted.ToString())
            {
                throw new UserInDeleteStatusException();
            }

            var result = await identityService.ChangePasswordAsync(request.Email, request.CurrentPassword, request.NewPassword);

			if (!result)
				throw new ChangePasswordWrongPasswordException();

			// send emails here
		}
	}
}
