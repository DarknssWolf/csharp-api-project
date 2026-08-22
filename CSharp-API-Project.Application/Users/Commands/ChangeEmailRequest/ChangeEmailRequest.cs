using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Users.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Commands.ChangeEmailRequest;

public class ChangeEmailRequestCommandResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
}

[Authorize (Roles = Roles.Administrator)]
[Authorize (Roles = Roles.Vendor)]
public class ChangeEmailRequestCommand : IRequest<ChangeEmailRequestCommandResponse>
{
	public string Email { get; set; }
	public string NewEmail { get; set; }
	public string ConfirmEmailChangeUrl { get; set; }
}

public class ChangeEmailRequestCommandHandler(
	IIdentityService identityService
) : IRequestHandler<ChangeEmailRequestCommand, ChangeEmailRequestCommandResponse>
{
	public async Task<ChangeEmailRequestCommandResponse> Handle(ChangeEmailRequestCommand request, CancellationToken cancellationToken)
	{
		var userId = await identityService.GetUserIdAsync(request.Email);
		if (userId == null)
			throw new UserNotFoundException(request.Email);

		var toUser = await identityService.GetUserIdAsync(request.NewEmail);
		if (toUser != null)
			throw new UserAlreadyExistsException(request.NewEmail);

        var userClaims = await identityService.GetClaimsAsync(userId);
        var userStatus = userClaims.FirstOrDefault(x => x.Type == "UserStatus")?.Value;
        if (userStatus == UserStatus.Deleted.ToString())
        {
            throw new UserInDeleteStatusException();
        }

        // Get the token to send to the user
        var token = await identityService.GenerateEmailChangeToken(userId, request.NewEmail);

		// Make the URL that the user will click on
		var param = new Dictionary<string, string?>
		{
			{ "token", token },
			{ "email", request.Email },
			{ "newMail", request.NewEmail }
		};

		var changeEmailConfirmUrl = QueryHelpers.AddQueryString(request.ConfirmEmailChangeUrl, param);

		// send emails here

		// Tell the front end that we have queued the mail
		return new ChangeEmailRequestCommandResponse
		{
			Success = true,
			Message = "Sent!"
		};
	}
}
