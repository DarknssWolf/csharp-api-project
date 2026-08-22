using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Users.Commands.UpdateUser;

[Authorize(Policy = Policies.CanUpdate_User)]
public class UpdateUserCommand : IRequest
{
	public Guid UserId { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string? Phone { get; set; }
	public string? LanguageISOCode { get; set; }
	public string? UserGroup { get; set; }
	public string? UserSubGroup { get; set; }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
	public UpdateUserCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();

		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage("FirstName is required.");

		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage("LastName is required.");
	}
}

public class UpdateUserCommandHandler(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IAuthHelper authHelper
) : IRequestHandler<UpdateUserCommand>
{
	public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
	{
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
        var isVendorAdmin = await authHelper.LoggedIn_IsVendorAdmin();

        var identity = isPlatformAdmin ? globalIdentityService : identityService;
        var userId = request.UserId.ToString();

        var userEmail = await identity.GetUserEmailAsync(userId);
        if (userEmail == null)
            throw new UserWithIdNotFoundException(request.UserId);

        // Vendor admins can only manage users within their own tenant.
        // Anyone else can only update their own details.
        if (isVendorAdmin && !isPlatformAdmin)
        {
            var loggedInUserTenantId = await identity.GetTenantId(user.Id);
            var userTenantId = await identity.GetTenantId(userId);

            if (loggedInUserTenantId != userTenantId)
                throw new InsufficientPermissionsToManageUserException();
        }
        else if (!isPlatformAdmin && user.Id != userId)
            throw new InsufficientPermissionsToManageUserException();

        await identity.SetClaims(userId, ClaimTypes.GivenName, request.FirstName);
        await identity.SetClaims(userId, ClaimTypes.Surname, request.LastName);

        if (request.Phone != null)
            await identity.ChangeUserPhoneNumberAsync(userId, request.Phone);

        if (!string.IsNullOrWhiteSpace(request.LanguageISOCode))
            await identity.SetClaims(userId, "Language", request.LanguageISOCode);

        if (!string.IsNullOrWhiteSpace(request.UserGroup))
            await identity.SetClaims(userId, "UserGroupId", request.UserGroup);

        if (request.UserSubGroup != null)
            await identity.SetClaims(userId, "UserSubGroupId", request.UserSubGroup);
    }
}
