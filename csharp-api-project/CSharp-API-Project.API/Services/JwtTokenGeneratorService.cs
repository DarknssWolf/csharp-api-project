using CSharpAPIProject.Application.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CSharp_API_Project.API.Services;

public class JwtTokenGeneratorService(
    IConfiguration configuration,
    ILogger<JwtTokenGeneratorService> logger
) : IJwtTokenGeneratorService
{
    public string GenerateToken(string userId, List<Claim> claims, bool longLived = false, DateTime? ExpiryDate = null)
    {
        var JWTKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(JWTKey))
            throw new InvalidOperationException("JWT Tokens can't be generated when the configuration key `Jwt:Key` is not present.");

        claims.AddRange(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }
        );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        string? sDefaultExpiry = configuration["Jwt:ExpiresInMinutes"];
        int defaultExpiryMinutes = 0;

        if (int.TryParse(sDefaultExpiry, out defaultExpiryMinutes) == false){
            defaultExpiryMinutes = 5; // I dont want your token to be active very long. You didnt tell me what the default time should be.
            logger.LogWarning("Jwt:ExpiresInMinutes key not set in configuration. JWT Token will be valid for 5 minutes!");
        }
        DateTime expiration;

        if (ExpiryDate != null)
            expiration = (DateTime)ExpiryDate; // Dis mos nie null nie, ons het gekyk, so ons kan hom cast na date time toe.
        else if (longLived)
            expiration = DateTime.MaxValue;
        else
            expiration = DateTime.Now.AddMinutes(defaultExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        claimsPrincipal = null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Use the same validation parameters as configured in the authentication middleware
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            // Validate the token
            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            // Token validation failed
            return false;
        }
    }
}
