using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.UserRoles.Exceptions;
using CSharpAPIProject.Application.UserRoles.Helpers.PromotionHelpers;

namespace CSharpAPIProject.Application.UserRoles.Commands.TakeRole;

public class TakeRoleCommand : IRequest
{
	public Guid UserId { get; set; }
	public string Role { get; set; }
}

public class TakeRoleCommandValidator : AbstractValidator<TakeRoleCommand>
{
	public TakeRoleCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Role).NotEmpty();
	}
}

public class TakeRoleCommandHandler(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IAuthHelper authHelper,
    IUser user
) : IRequestHandler<TakeRoleCommand>
{
	public async Task Handle(TakeRoleCommand request, CancellationToken cancellationToken)
	{
		if (user == null)
			throw new UserNotLoggedInException();

		var roles = await identityService.GetRolesAsync(user);

		// If you don't have any roles then you can't promote anyone.  
		if (roles == null)
			throw new InsufficientPermissionsToTakeRoleException(request.Role);

		var rolesAllowedToTake = PromotionHelper.GetValidRolesToGive(roles.ToList());

		// If you don't have permission to take the role then you can't take it.
		if (!rolesAllowedToTake.Contains(request.Role))
			throw new InsufficientPermissionsToGiveRoleException(request.Role);

        // We are just going to see if we are able to find this user.
        // We don't actually care about the email address, it must just not be null.
        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        bool isClientUser = await authHelper.LoggedIn_IsVendorAdmin();

        string? userEmailToAssignRoleTo;
        if (isPlatformAdmin)
            userEmailToAssignRoleTo = await globalIdentityService.GetUserEmailAsync(request.UserId.ToString());
        else
            userEmailToAssignRoleTo = await identityService.GetUserEmailAsync(request.UserId.ToString());

        if (userEmailToAssignRoleTo == null)
			throw new UserNotFoundException(request.UserId);

        // Make sure we don't remove the last role from the user.
        IList<string>? rolesForUser;
        if (isPlatformAdmin)
            rolesForUser = await globalIdentityService.GetRolesAsync(request.UserId);
        else
            rolesForUser = await identityService.GetRolesAsync(request.UserId);

        if (rolesForUser == null || rolesForUser.Count == 1)
			throw new CantRemoveLastRoleForUserException();

        bool removedFromRole = false;
        if (isPlatformAdmin)
            removedFromRole = await globalIdentityService.RemoveFromRole(request.UserId.ToString(), request.Role);
        else
            removedFromRole = await identityService.RemoveFromRole(request.UserId.ToString(), request.Role);

        if (!removedFromRole)
			throw new BadRequestException($"The role {request.Role} could not be removed from the user with the id {request.UserId}");
	}
}