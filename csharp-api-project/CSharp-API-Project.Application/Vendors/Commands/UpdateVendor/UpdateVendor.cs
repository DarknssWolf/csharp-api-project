using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Common.Regex;
using CSharpAPIProject.Application.Interfaces.Entities;

namespace CSharpAPIProject.Application.Vendors.Commands.UpdateVendor;

[Authorize(Policy = Policies.CanUpdate_Vendor)]
public class UpdateVendorCommand : IRequest
{
	public int VendorId { get; set; }
	public string VendorName { get; set; }
	public string Email { get; set; }
	public string WebhookUrl { get; set; }
	public string WebhookSecret { get; set; }
	public string ContactName { get; set; }
	public string ContactEmail { get; set; }
	public string FinanceEmail { get; set; }
	public Guid AccountManager { get; set; }
	public int DecimalRounding { get; set; }
	public bool ParticipantPaysHandlingFee { get; set; }
	public bool SplitFees { get; set; }
    public string VendorDomainUrl { get; set; }
}

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.VendorName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.WebhookUrl)
            .Matches(RegexValidation.URLValidationPattern)
            .WithMessage("Invalid URL format. Ensure that your top level domain ends in a valid suffix and that you are not trying to specify the port.")
            .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl));

        RuleFor(x => x.WebhookSecret).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl));

        RuleFor(x => x.ContactName).NotEmpty();
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.FinanceEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AccountManager).NotEmpty();
        RuleFor(x => x.DecimalRounding).NotEmpty();
        RuleFor(x => x.ParticipantPaysHandlingFee).Must(x => x == true || x == false).WithMessage("ParticipantPaysHandlingFee must be a boolean value.");
        RuleFor(x => x.SplitFees).Must(x => x == true || x == false).WithMessage("SplitFees must be a boolean value.");
        RuleFor(x => x.VendorDomainUrl)
            .NotEmpty()
            .Matches(RegexValidation.URLValidationPattern)
            .WithMessage("Invalid URL format. Ensure that your top level domain ends in a valid suffix and that you are not trying to specify the port.")
            .When(x => !string.IsNullOrWhiteSpace(x.VendorDomainUrl));
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

        bool isTICUser = await authHelper.LoggedIn_IsTICUser();
		if (!isTICUser)
			vendorQuery = vendorQuery.Where(x => x.VendorId == tenantResolver.ResolveTenantId());

		var vendor = vendorQuery.FirstOrDefault();

		if (vendor == null)
			throw new VendorNotFoundException(request.VendorId);

		vendor.Name = request.VendorName;
		vendor.Email = request.Email;
		vendor.WebhookUrl = request.WebhookUrl;
		vendor.WebhookSecret = request.WebhookSecret;
		vendor.ContactName = request.ContactName;
		vendor.ContactEmail = request.ContactEmail;
		vendor.FinanceEmail = request.FinanceEmail;
		vendor.AccountManager = request.AccountManager;
		vendor.DecimalRounding = request.DecimalRounding;
		vendor.PariticpantPaysHandlingFee = request.ParticipantPaysHandlingFee;
		vendor.SplitFees = request.SplitFees;
        vendor.DomainUrl = request.VendorDomainUrl;

        await context.SaveChangesAsync(cancellationToken);

	}
}