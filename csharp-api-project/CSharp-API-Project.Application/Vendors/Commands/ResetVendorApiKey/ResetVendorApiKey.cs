using System.Security.Claims;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Application.Vendors.Exceptions;
using Microsoft.Extensions.Logging;

namespace CSharpAPIProject.Application.Vendors.Commands.ResetVendorApiKey;

public class ResetVendorApiKeyCommandResponse
{
    public bool Success { get; set; } = false;
    public string? ApiKey { get; set; }
}

[Authorize(Policy = Policies.CanReset_VendorApiKey)]
public class ResetVendorApiKeyCommand : IRequest<ResetVendorApiKeyCommandResponse>
{
    public int vendorId { get; set; }
}

public class ResetVendorApiKeyCommandValidator : AbstractValidator<ResetVendorApiKeyCommand>
{
    public ResetVendorApiKeyCommandValidator()
    {
        RuleFor(x => x.vendorId)
            .NotNull()
            .WithMessage("Vendor ID is required.");
    }
}

public class ResetVendorApiKeyCommandHandler(
    IJwtTokenGeneratorService tokenService,
    IApplicationDbContext context,
    IAuthHelper authHelper,
    ILogger<ResetVendorApiKeyCommandHandler> logger,
    IGlobalIdentityService globalIdentityService
) : IRequestHandler<ResetVendorApiKeyCommand, ResetVendorApiKeyCommandResponse>
{
    public async Task<ResetVendorApiKeyCommandResponse> Handle(ResetVendorApiKeyCommand request, CancellationToken cancellationToken)
    {
        var isTICUser = await authHelper.LoggedIn_IsTICUser();

        var vendorQuery = context.Vendors.Where(v => v.VendorId == request.vendorId);
        if (isTICUser)
            vendorQuery = vendorQuery.IgnoreQueryFilters();

        var vendor = await vendorQuery.FirstOrDefaultAsync(cancellationToken);

        if(vendor == null)
            throw new VendorNotFoundException(request.vendorId);

        var apiUserId = await globalIdentityService.GetVendorApiUserAsync(vendor.VendorId);

        if(apiUserId == null)
        {
            // If there wasnt an API user for this vendor, lets create one and log this event.
            logger.LogWarning("No API user found for vendor {VendorId}. Creating a new API user.", vendor.VendorId);

            apiUserId = await globalIdentityService.CreateVendorApiUserAsync(vendor.VendorId);

            if (string.IsNullOrEmpty(apiUserId))
            {
                logger.LogError("Failed to create API user for vendor {VendorId}.", vendor.VendorId);
                throw new ApiUserCreationFailed(vendor.VendorId);
            }
        }

        await globalIdentityService.RemoveApiKeyAsync(apiUserId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, Roles.VendorApi),
            new Claim("VendorId", vendor.VendorId.ToString())
        };
        var apiKey = tokenService.GenerateToken(apiUserId, claims, true);

        var setApiKey = await globalIdentityService.SetApiKeyAsync(apiUserId, apiKey);

        if (!setApiKey)
        {
            logger.LogError("Failed to set API key for vendor {VendorId}.", vendor.VendorId);
            throw new ApiUserCreationFailed(vendor.VendorId);
        }

        var newApiKey = await globalIdentityService.GetVendorApiKeyAsync(vendor.VendorId);

        if (string.IsNullOrEmpty(newApiKey))
        {
            logger.LogError("Failed to retrieve new API key for vendor {VendorId}.", vendor.VendorId);
            throw new ApiUserCreationFailed(vendor.VendorId);
        }

        logger.LogInformation("Successfully reset API key for vendor {VendorId}.", vendor.VendorId);
        return new ResetVendorApiKeyCommandResponse()
        {
            Success = true,
            ApiKey = newApiKey
        };
    }
}