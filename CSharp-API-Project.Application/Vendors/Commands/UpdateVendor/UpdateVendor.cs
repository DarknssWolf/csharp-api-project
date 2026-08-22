using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.Vendors.Commands.UpdateVendor;

[Authorize(Policy = Policies.CanUpdate_Vendor)]
public class UpdateVendorCommand : IRequest
{
	public int VendorId { get; set; }
	public string VendorName { get; set; }
	public string Email { get; set; }
	public string ContactName { get; set; }
	public string ContactEmail { get; set; }
}

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.VendorName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.ContactName).NotEmpty();
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
    }
}

public class UpdateVendorCommandHandler(
	IApplicationDbContext context,
	IUser user,
	IIdentityService identityService,
	ITenantResolver tenantResolver,
    IAuthHelper authHelper
) : IRequestHandler<UpdateVendorCommand>
{
	public async Task Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
	{
		var vendorQuery = context.Vendors.Where(x => x.VendorId == request.VendorId);

        bool isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();
		if (!isPlatformAdmin)
			vendorQuery = vendorQuery.Where(x => x.VendorId == tenantResolver.ResolveTenantId());

		var vendor = vendorQuery.FirstOrDefault();

		if (vendor == null)
			throw new VendorNotFoundException(request.VendorId);

		vendor.Name = request.VendorName;
		vendor.Email = request.Email;
		vendor.ContactName = request.ContactName;
		vendor.ContactEmail = request.ContactEmail;

        await context.SaveChangesAsync(cancellationToken);

	}
}