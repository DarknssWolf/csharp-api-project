using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using System.Security.Claims;
using CSharpAPIProject.Application.Common.Security;

namespace CSharpAPIProject.Application.Vendors.Commands.RegisterVendor;

[Authorize(Policy = Policies.CanCreate_Vendor)]
public class RegisterVendorCommand() : IRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
}

public class RegisterVendorCommandValidator : AbstractValidator<RegisterVendorCommand>
{
    public RegisterVendorCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ContactName).NotEmpty();
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
    }
}
public class RegisterVendorCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IIdentityService identityService,
    ITenantResolver tenantResolver) :
    IRequestHandler<RegisterVendorCommand>
{
    public async Task Handle(RegisterVendorCommand request, CancellationToken cancellationToken)
    {
        // Check if this email is already in use
        var vendor = await context.Vendors.Where(x => x.Email == request.Email).FirstOrDefaultAsync(cancellationToken);

        if (vendor != null)
        {
            throw new EmailAlreadyInUseException();
        }

        // Create a new vendor
        var newVendor = new Vendor
        {
            Email = request.Email,
            Name = request.Name,
            Code = Guid.NewGuid().ToString("N").Substring(0, 32),
            CreatedOn = DateTime.UtcNow,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
        };
        context.Vendors.Add(newVendor);
        await context.SaveChangesAsync(cancellationToken);

        newVendor = await context.Vendors.FirstOrDefaultAsync(x => x.Email == request.Email);

        tenantResolver.SetTenantId(newVendor.VendorId);

        // Create User
        var userResult = await CreateVendorUser(newVendor.Email, tenantResolver.ResolveTenantId(), request.Password);

        if (!userResult)
            throw new Exception("Something went wrong");

        // Create a default region for all customers to be registered to.
        var defaultRegion = new Region
        {
            RegionCode = "DEFAULT",
            RegionName = "Default Region",
            VendorId = newVendor.VendorId,
        };

        context.Regions.Add(defaultRegion);
        await context.SaveChangesAsync(cancellationToken);

        // send emails here
    }
    private async Task<bool> CreateVendorUser(string username, int vendorId, string password)
    {
        (var result, var userId) = await identityService.CreateUserAsync(username, password, vendorId);
        if (!result.Succeeded)
            return false;

        var authorizeResult = await identityService.AddToRoleAsync(userId, Roles.Vendor);

        if (!authorizeResult)
            return false;

        var addClaim = await identityService.AddClaimAsync(userId, new Claim("VendorId", vendorId.ToString()));
        if (!addClaim)
            return false;
        return true;
    }
}
