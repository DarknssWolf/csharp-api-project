using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.UserRoles.Exceptions;
using CSharpAPIProject.Application.UserRoles.Helpers.PromotionHelpers;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.UserRoles.Commands.GiveRole;

public class GiveRoleCommand : IRequest
{
	public Guid UserId { get; set; }
	public string Role { get; set; }
}

public class GiveRoleCommandValidator : AbstractValidator<GiveRoleCommand>
{
	public GiveRoleCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Role).NotEmpty();
	}
}

public class GiveRoleCommandHandler(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IAuthHelper authHelper
) : IRequestHandler<GiveRoleCommand>
{
	public async Task Handle(GiveRoleCommand request, CancellationToken cancellationToken)
	{
		if (user == null)
			throw new UserNotLoggedInException();

		var roles = await identityService.GetRolesAsync(user);

		// If you don't have any roles then you can't promote anyone.  
		if (roles == null)
			throw new InsufficientPermissionsToGiveRoleException(request.Role);

        var rolesAllowedToAssign = PromotionHelper.GetValidRolesToGive(roles.ToList());

		// If you don't have permission to give the role then you can't give it.
		if (!rolesAllowedToAssign.Contains(request.Role))
			throw new InsufficientPermissionsToGiveRoleException(request.Role);

        bool isTICUser = await authHelper.LoggedIn_IsTICUser();
        bool isClientUser = await authHelper.LoggedIn_IsClientAdmin();
        // We are just going to see if we are able to find this user.
        // We don't actually care about the email address, it must just not be null.
        string? userEmailToAssignRoleTo;

        if(isTICUser)
            userEmailToAssignRoleTo = await globalIdentityService.GetUserEmailAsync(request.UserId.ToString());
        else
            userEmailToAssignRoleTo = await identityService.GetUserEmailAsync(request.UserId.ToString());

        if (userEmailToAssignRoleTo == null)
			throw new UserNotFoundException(request.UserId);

        if (isClientUser)
        {
            var loggedInUserTenantId = await identityService.GetTenantId(user.Id);
            var userTenantId = await identityService.GetTenantId(request.UserId.ToString());

            if (loggedInUserTenantId != userTenantId)
                throw new InsufficientPermissionsToGiveRoleException(request.Role);
        }

        bool assignedToRole = false;
        if (isTICUser)
            assignedToRole = await globalIdentityService.AddToRoleAsync(request.UserId.ToString(), request.Role);
        else
            assignedToRole = await identityService.AddToRoleAsync(request.UserId.ToString(), request.Role);

        if (!assignedToRole)
			throw new BadRequestException($"The role {request.Role} could not be assigned to the user with the id {request.UserId}");
	}
}