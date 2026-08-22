using System.Security.Claims;
using CSharpAPIProject.Application.Common.Models;

namespace CSharpAPIProject.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<bool> LoginUserAsync(string email, string password, int tenantId);
    Task<bool> IsUser(string email);
    Task<string?> GetUserIdAsync(string email);
    Task<string?> GetUserIdAsync(string email, int tenantId);
    string? GetUserId(string email, int TenantId);
    Task<string?> GetUserEmailAsync(string userId);
    Task<int> GetTenantId(string userId);
    Task<(IReadOnlyCollection<UserAccount> Users, int TotalCount)> GetUsersForVendorAsync(int vendorId, string? queryFilter, int pageNumber, int pageSize);
    Task<IList<string>?> GetRolesAsync(string email, int tenantId);
    Task<IList<string>?> GetRolesAsync(IUser user);
    Task<IList<string>?> GetRolesAsync(Guid userId);
    Task<IList<Claim>?> GetClaimsAsync(Guid userId);
    Task<IList<Claim>?> GetClaimsAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int tenantId);
    Task<bool> AddToRoleAsync(string userId, string role);
    Task<bool> RemoveFromRole(string userId, string role);
    Task<bool> AddClaimAsync(string userId, Claim claim);
    Task<bool> SetClaims(string userId, string claimName, string claimValue);
    Task ChangeUserPhoneNumberAsync(string userId, string phoneNumber);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetToken(string userId);
    Task<string> GenerateEmailChangeToken(string userId, string newEmail);
    Task<bool> ChangeEmailAddressAsync(string userId, string newEmail, string changeToken);
    Task<string?> SaveRefreshTokenAsync(string userId, string refreshToken, string expiryTime);
    Task<string?> GetRefreshToken(string userId);
    Task<string?> GetRefreshTokenExpiry(string userId);
    Task<bool> RevokeRefreshToken(string userId);

} 