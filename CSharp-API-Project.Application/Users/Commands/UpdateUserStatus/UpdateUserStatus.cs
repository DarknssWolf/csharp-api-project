using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Commands.UpdateUserStatus;

[Authorize(Policy = Policies.CanUpdate_User)]
public class UpdateUserStatusCommand : IRequest
{
	public Guid UserId { get; set; }
	public UserStatus Status { get; set; }
}

public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
	public UpdateUserStatusCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Status).IsInEnum();
	}
}

public class UpdateUserStatusCommandHandler(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IAuthHelper authHelper
) : IRequestHandler<UpdateUserStatusCommand>
{
	public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
	{
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isVendorAdmin = await authHelper.LoggedIn_IsVendorAdmin();

        // Changing a user's status is an admin action, self service users can't do it.
        if (!isPlatformAdmin && !isVendorAdmin)
            throw new InsufficientPermissionsToManageUserException();

        var identity = isPlatformAdmin ? globalIdentityService : identityService;
        var userId = request.UserId.ToString();

        var userEmail = await identity.GetUserEmailAsync(userId);
        if (userEmail == null)
            throw new UserWithIdNotFoundException(request.UserId);

        // Vendor admins can only manage users within their own tenant.
        if (!isPlatformAdmin)
        {
            var loggedInUserTenantId = await identity.GetTenantId(user.Id);
            var userTenantId = await identity.GetTenantId(userId);

            if (loggedInUserTenantId != userTenantId)
                throw new InsufficientPermissionsToManageUserException();
        }

        await identity.SetClaims(userId, "UserStatus", request.Status.ToString());

        // Anything other than active should invalidate the user's refresh token.
        if (request.Status != UserStatus.Active)
            await identity.RevokeRefreshToken(userId);
    }
}
