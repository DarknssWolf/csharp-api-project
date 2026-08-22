using System.Security.Claims;

namespace CSharpAPIProject.Application.Interfaces.Services;

public interface IJwtTokenGeneratorService
{
    string GenerateToken(string userId, List<Claim> claims, bool longLived = false, DateTime? ExpiryDate = null);
    bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal);
}
