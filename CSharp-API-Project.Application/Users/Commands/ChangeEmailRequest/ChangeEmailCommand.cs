using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.Users.Commands.ChangeEmailRequest;

public class ChangeEmailCommandResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
}

[Authorize (Roles = Roles.Administrator)]
[Authorize (Roles = Roles.Vendor)]
public class ChangeEmailCommand : IRequest<ChangeEmailCommandResponse>
{
	public string OldEmail { get; set; }
	public string NewEmail { get; set; }
	public string ChangeToken { get; set; }
}

public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
	public ChangeEmailCommandValidator()
	{
		RuleFor(x => x.OldEmail).NotEmpty().EmailAddress();
		RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
		RuleFor(x => x.ChangeToken).NotEmpty();
	}
}

public class ChangeEmailCommandHandler(
	IIdentityService identityService
) : IRequestHandler<ChangeEmailCommand, ChangeEmailCommandResponse>
{
	public async Task<ChangeEmailCommandResponse> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
	{
		var fromUserId = await identityService.GetUserIdAsync(request.OldEmail);
		if (fromUserId == null)
			throw new UserNotFoundException(request.OldEmail);

		var toUserId = await identityService.GetUserIdAsync(request.NewEmail);
		if (toUserId != null)
			throw new UserAlreadyExistsException(request.NewEmail);

		var result = await identityService.ChangeEmailAddressAsync(fromUserId, request.NewEmail, request.ChangeToken);
		if (!result)
			return new ChangeEmailCommandResponse
			{
				Success = false,
				Message = "Failed to change email address."
			};

		// send emails here

		return new ChangeEmailCommandResponse
		{
			Success = true,
			Message = "Email changed successfully."
		};
	}
}
