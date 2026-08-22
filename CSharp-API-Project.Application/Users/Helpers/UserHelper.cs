using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Users.Exceptions;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Users.Helpers;

public class UserHelper
{
    public record UserLoginToken
    {
        public string Token;
        public DateTime Expiry;
    }

    public static UserLoginToken CreateUserLoginToken(
        string userId,
        IList<string> roles,
        string tenantId,
        IJwtTokenGeneratorService tokenService,
        IConfiguration configuration,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        try
        {
            foreach (var role in roles)
            {
                if (role == "Administrator") claims.Add(new Claim("AdministratorId", tenantId));
                if (role == "Vendor") claims.Add(new Claim("VendorId", tenantId));
                if (role == "Customer") claims.Add(new Claim("CustomerId", tenantId));
            }
        }
        catch (NotImplementedException)
        {
            throw new BadRequestException("Permissions for this role has not yet been implimented.");
        }

        // Add any additional claims if provided
        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        // Expiry Time
        var expiryTime = configuration.GetValue<int>("Jwt:ExpiresInMinutes");
        if (expiryTime < 0) expiryTime = 30;
        DateTime expiryDate = DateTime.UtcNow.AddMinutes(expiryTime);

        // Create the token and respond.
        var token = tokenService.GenerateToken(userId, claims);
        return new UserLoginToken() { Token = token, Expiry = expiryDate };
    }

    public static async Task<string> ResolveTenantIdAsync(
        string userEmail,
        IList<string> roles,
        IApplicationDbContext context,
        int vendorId)
    {
        var tenantId = string.Empty;

        // Determine which roles the user belongs to and add claims accordingly
        var roleChecks = new Dictionary<string, Func<Task>>
        {
            { Roles.Administrator, async () => {
                tenantId = "0"; // Administrator always has tenant ID 0
            }},
            { Roles.Vendor, async () => {
                var vendor = await context.Vendors.SingleOrDefaultAsync(x => x.VendorId == vendorId);
                if (vendor == null) throw new UserDataEntityNotFound("Vendor");
                tenantId = vendor.VendorId.ToString();
            }},
            { Roles.Customer, async () => {
                var customer = await context.Customers.SingleOrDefaultAsync(x => x.Email == userEmail);
                if (customer == null) throw new UserDataEntityNotFound("Customer");
                tenantId = customer.CustomerId.ToString();
            }}
        };

        if (roles != null)
            foreach (var role in roles)
                if (roleChecks.ContainsKey(role)) await roleChecks[role]();

        return tenantId;
    }

    public static async Task ValidateUserStatusAsync(
        IList<Claim> userClaims)
    {
        var userStatus = userClaims?.FirstOrDefault(x => x.Type == "UserStatus")?.Value;
        if (userStatus == UserStatus.Deleted.ToString() || userStatus == UserStatus.Suspended.ToString())
            throw new UserInDeleteStatusException();
    }

    public static async Task<AccessTokenResponse> CreateAccessTokenResponseAsync(
        string userId,
        IList<string> roles,
        string tenantId,
        IJwtTokenGeneratorService tokenService,
        IConfiguration configuration,
        IIdentityService identityService,
        IEnumerable<Claim>? additionalClaims = null,
        bool generateRefreshTokens = true
    )
    {
        var loginToken = CreateUserLoginToken(userId, roles, tenantId, tokenService, configuration, additionalClaims);
        string refreshToken = string.Empty;

        if (generateRefreshTokens)
        {
            var expiryDate = DateTime.UtcNow.AddDays(7);
            var expiryTime = expiryDate.ToString("o");

            refreshToken = tokenService.GenerateToken(userId, [], false, expiryDate);

            await identityService.RevokeRefreshToken(userId);
            await identityService.SaveRefreshTokenAsync(userId, refreshToken, expiryTime);

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Failed to generate refresh token.");
        }

        return new AccessTokenResponse
        {
            AccessToken = loginToken.Token,
            RefreshToken = refreshToken,
            ExpiresIn = loginToken.Expiry
        };
    }
}
