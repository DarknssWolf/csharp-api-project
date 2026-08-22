using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Commands.DeleteUser;

[Authorize(Policy = Policies.CanDelete_User)]
public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
	public DeleteUserCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
	}
}

public class DeleteUserCommandHandler(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IAuthHelper authHelper
) : IRequestHandler<DeleteUserCommand>
{
	public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        var identity = isPlatformAdmin ? globalIdentityService : identityService;
        var userId = request.UserId.ToString();

        var userEmail = await identity.GetUserEmailAsync(userId);
        if (userEmail == null)
            throw new UserWithIdNotFoundException(request.UserId);

        // Vendor admins can only delete users within their own tenant.
        if (!isPlatformAdmin)
        {
            var loggedInUserTenantId = await identity.GetTenantId(user.Id);
            var userTenantId = await identity.GetTenantId(userId);

            if (loggedInUserTenantId != userTenantId)
                throw new InsufficientPermissionsToManageUserException();
        }

        // We don't delete the account, the user claim is set to deleted.
        await identity.SetClaims(userId, "UserStatus", UserStatus.Deleted.ToString());
        await identity.RevokeRefreshToken(userId);
    }
}
