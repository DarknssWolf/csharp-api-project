using System.Security.Claims;
using CSharpAPIProject.Application.Common.Models;

namespace CSharpAPIProject.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<bool> LoginUserAsync(string email, string password, int tenantId);
    Task<string?> GetUserIdAsync(string email, int tenantId);
    Task<IList<string>?> GetRolesAsync(string email, int tenantId);
    Task<IList<Claim>?> GetClaimsAsync(Guid userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int tenantId);
    Task<bool> AddToRoleAsync(string userId, string role);
    Task<bool> AddClaimAsync(string userId, Claim claim);
    Task<string?> SaveRefreshTokenAsync(string userId, string refreshToken, string expiryTime);
    Task<string?> GetRefreshToken(string userId);
    Task<string?> GetRefreshTokenExpiry(string userId);
    Task<bool> RevokeRefreshToken(string userId);

} 