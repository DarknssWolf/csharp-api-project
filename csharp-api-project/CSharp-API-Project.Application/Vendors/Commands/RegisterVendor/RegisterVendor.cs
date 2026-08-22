using Microsoft.Extensions.Logging;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using System.Security.Claims;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.ValueObjects;
using CSharpAPIProject.Domain.Common.Regex;

namespace CSharpAPIProject.Application.Vendors.Commands.RegisterVendor;

[Authorize(Policy = Policies.CanCreate_Vendor)]
public class RegisterVendorCommand() : IRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
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
public class RegisterVendorCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IIdentityService identityService,
    IEmailService emailSender,
    IJwtTokenGeneratorService tokenService,
    IOtpGeneratorService otpService,
    ILogger<RegisterVendorCommandHandler> logger,
    ITenantResolver tenantResolver) :
    IRequestHandler<RegisterVendorCommand>
{
    public async Task Handle(RegisterVendorCommand request, CancellationToken cancellationToken)
    {
        // Check if this email is already in use (supplier and vendor tables)

        var supplier = context.Suppliers.Where(x => x.Email == request.Email).FutureValue();
        var vendor = context.Vendors.Where(x => x.Email == request.Email).FutureValue();

        if (supplier.Value != null || vendor.Value != null)
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
            WebhookUrl = request.WebhookUrl,
            WebhookSecret = request.WebhookSecret,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            FinanceEmail = request.FinanceEmail,
            AccountManager = request.AccountManager,
            DecimalRounding = request.DecimalRounding,
            PariticpantPaysHandlingFee = request.ParticipantPaysHandlingFee,
            SplitFees = request.SplitFees,
            DomainUrl = request.VendorDomainUrl,
        };
        context.Vendors.Add(newVendor);
        await context.SaveChangesAsync(cancellationToken);

        newVendor = await context.Vendors.FirstOrDefaultAsync(x => x.Email == request.Email);

        tenantResolver.SetTenantId(newVendor.VendorId);

        // Create User
        var userResult = await CreateVendorUser(newVendor.Email, tenantResolver.ResolveTenantId(), request.Password);

        if (!userResult)
            throw new Exception("Something went wrong");

        // Create Api User
        var apiUser = await identityService.CreateVendorApiUserAsync(newVendor.VendorId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, Roles.VendorApi),
            new Claim("VendorId", newVendor.VendorId.ToString())
        };
        var apiKey = tokenService.GenerateToken(apiUser, claims, true);

        var setApiKey = await identityService.SetApiKeyAsync(apiUser, apiKey);

        // Create a default region for all customers to be registered to.
        var defaultRegion = new Region
        {
            BaseCurrency = Currency.SouthAfricaRand,
            RegionCode = "DEFAULT",
            RegionName = "Default Region",
            VendorId = newVendor.VendorId,
        };

        context.Regions.Add(defaultRegion);
        await context.SaveChangesAsync(cancellationToken);

        // todo: Email functionality
        // Do the necessary to inform and supply the supplier with a username and password to start using the portal and api
        //var emailSuccess = await emailSender.SendEmailAsync(request.Email,"Welcome to CSharpAPIProject", "Your account is now active. You can log in using the credentials you have signed up with.<br>The TIC Team.");

        //if (!emailSuccess)
        //	logger.LogError(new UserEmailSendFailedException(request.Email), "Email send failed");


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
