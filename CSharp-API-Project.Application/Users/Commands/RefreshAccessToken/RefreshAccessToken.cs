using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Application.Users.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Users.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandResponse : AccessTokenResponse {
	public bool Success { get; set; } = false;
	public string Message { get; set; }
}

public class RefreshAccessTokenCommand : IRequest<RefreshAccessTokenCommandResponse>
{
	public string RefreshToken { get; set; }
}

public class RefreshAccessTokenCommandValidator : AbstractValidator<RefreshAccessTokenCommand>
{
	public RefreshAccessTokenCommandValidator()
	{
		RuleFor(x => x.RefreshToken).NotEmpty();
	}
}

public class RefreshAccessTokenCommandHandler(
	IUser user,
	IIdentityService identityService,
	IJwtTokenGeneratorService tokenService,
	IApplicationDbContext context,
	ITenantResolver tenantResolver,
	IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenCommandResponse>
{
    public async Task<RefreshAccessTokenCommandResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id;

        if (userId == null)
        {
            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Authorization header is missing or invalid.");
            }

            var token = request.RefreshToken;

            // Use proper JWT validation instead of just reading the token
            // This validates:
            // - Token signature (prevents tampering)
            // - Token expiration (prevents use of expired tokens)
            // - Issuer and audience (ensures token was issued by this application)
            // - Token format and structure
            if (!tokenService.ValidateToken(token, out ClaimsPrincipal? claimsPrincipal))
            {
                throw new UnauthorizedAccessException("Invalid JWT token. Token may be expired, tampered with, or have an invalid signature.");
            }

            if (claimsPrincipal == null)
            {
                throw new UnauthorizedAccessException("Failed to extract claims from JWT token.");
            }

            var subClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subClaim == null)
            {
                throw new UnauthorizedAccessException("JWT token does not contain 'sub' claim.");
            }

            userId = subClaim.Value;
        }

        var vendorId = tenantResolver.ResolveTenantId();

        if(vendorId > 0)
        {
            Vendor? vendor;

            vendor = await context.Vendors.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
            if (vendor == null)
                throw new VendorNotFoundException(vendorId);
        }

        var userRefreshToken = await identityService.GetRefreshToken(userId);
        if (userRefreshToken == null) userRefreshToken = "";

        if (userRefreshToken != request.RefreshToken)
            throw new InvalidRefreshTokenException();

        var storedExpiry = await identityService.GetRefreshTokenExpiry(userId);
        if (storedExpiry == null)
        {
            await identityService.RevokeRefreshToken(userId);
            throw new InvalidRefreshExpirationException();
        }

        if (DateTime.TryParse(storedExpiry, out var expiryTime))
        {
            if (DateTime.UtcNow > expiryTime)
            {
                await identityService.RevokeRefreshToken(userId);
                throw new RefreshTokenExpiredException();
            }
            else
            {
                if(!Guid.TryParse(userId, out Guid userIdAsGuid))
                    throw new InvalidUserIdException();

                var roles = await identityService.GetRolesAsync(userIdAsGuid);

                // Get user email for tenant resolution
                var userClaims = await identityService.GetClaimsAsync(userIdAsGuid);
                var userEmail = userClaims?.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userEmail))
                    throw new InvalidUserIdException();

                // Resolve tenant ID using UserHelper
                var tenantId = await UserHelper.ResolveTenantIdAsync(userEmail, roles, context, vendorId);

                // Create access token response using UserHelper
                var accessTokenResponse = await UserHelper.CreateAccessTokenResponseAsync(
                    userId,
                    roles,
                    tenantId,
                    tokenService,
                    configuration,
                    identityService);

                return new RefreshAccessTokenCommandResponse
                {
                    AccessToken = accessTokenResponse.AccessToken,
                    ExpiresIn = accessTokenResponse.ExpiresIn,
                    RefreshToken = accessTokenResponse.RefreshToken,
                    Success = true
                };
            }
        }
        else
        {
            await identityService.RevokeRefreshToken(userId);
            throw new InvalidRefreshExpirationException();
        }
    }
}
