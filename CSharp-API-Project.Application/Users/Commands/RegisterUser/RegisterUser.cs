using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using System.Reflection;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Users.Commands.RegisterUser;

[Authorize(Policy = Policies.CanCreate_User)]
public class RegisterUserCommand : IRequest
{
	public int VendorId { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public string Role { get; set; }
    public string UserGroup { get; set; }
	public string? UserSubGroup { get; set; }
	public string? Phone { get; set; }

}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
	public RegisterUserCommandValidator()
	{
		RuleFor(v => v.FirstName)
			.NotEmpty()
			.WithMessage("FirstName is required.");

		RuleFor(v => v.LastName)
			.NotEmpty()
			.WithMessage("LastName is required.");

		RuleFor(v => v.Email)
			.NotEmpty()
			.WithMessage("Email is required.");

        RuleFor(v => v.Password)
			.NotEmpty()
			.WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");

		RuleFor(v => v.Role)
			.NotEmpty()
			.WithMessage("Role is required.");
	}
}

public class RegisterUserCommandHandler(
	IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
	ITenantResolver tenantResolver,
    IAuthHelper authHelper
) : IRequestHandler<RegisterUserCommand>
{
    private static readonly List<string> AllowedRolesToRegister = new List<string>
    {
        Roles.Vendor
    };

	public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
	{
		var roles = typeof(Roles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
								 .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
								 .Select(fi => fi.GetValue(null)?.ToString())
								 .ToList();

		// Check if request.Role is a valid role
		if (!roles.Contains(request.Role))
			throw new RoleDoesNotExistException(request.Role);

		// Check if the role is allowed to be registered
		if (!AllowedRolesToRegister.Contains(request.Role))
			throw new RoleDoesNotExistException(request.Role);

		// Get the user with this email
		var existingUsrId = await identityService.GetUserIdAsync(request.Email);
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

		var vendorId = isPlatformAdmin ? request.VendorId : tenantResolver.ResolveTenantId();

		// If there is no user with this email then we will register it
		if (existingUsrId == null)
		{
			var createUserReq = await identityService.CreateUserAsync(request.Email, request.Password, vendorId);
			if (!createUserReq.Result.Succeeded)
				throw new BadRequestException(string.Join("\n", createUserReq.Result.Errors));

            if(isPlatformAdmin)
                existingUsrId = await globalIdentityService.GetUserIdAsync(request.Email, vendorId);
            else
                existingUsrId = await identityService.GetUserIdAsync(request.Email);

			if(existingUsrId == null)
				throw new BadRequestException("User not found after creation.");

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.GivenName, request.FirstName),
				new Claim(ClaimTypes.Surname, request.LastName),
				new Claim(ClaimTypes.Email, request.Email),
				new Claim("UserStatus", UserStatus.Active.ToString()),
                new Claim("UserGroupId", request.UserGroup),
                new Claim("UserSubGroupId", request.UserSubGroup ?? "")
            };

            if(isPlatformAdmin)
			    foreach (var claim in claims)
				    await globalIdentityService.AddClaimAsync(existingUsrId, claim);
            else
                foreach(var claim in claims)
                    await identityService.AddClaimAsync(existingUsrId, claim);
        }

		// Check if the user is already in the role
		var isInRole = await identityService.IsInRoleAsync(existingUsrId, request.Role);
		if (isInRole)
			throw new UserAlreadyInRoleException(request.Role);

        bool addedToRole = false;
        if (isPlatformAdmin)
            addedToRole = await globalIdentityService.AddToRoleAsync(existingUsrId, request.Role);
        else
		    // We always want to make sure we add the user to the role.
		    addedToRole = await identityService.AddToRoleAsync(existingUsrId, request.Role);

		// If the add to the role didn't work then lets inform the user
		if (!addedToRole)
			throw new FailedToAddUserToRoleException(request.Role);

		// send emails here
	}
}
