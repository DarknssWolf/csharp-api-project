using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CSharpAPIProject.Domain.Common;
using System.Security.Claims;
using Z.EntityFramework.Plus;
using CSharpAPIProject.Infrastructure.Identity.Tenant;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using CSharpAPIProject.Application.Models;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Identity;
using System.Linq;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Infrastructure.Data;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CSharpAPIProject.Infrastructure.Identity;

/// <summary>
/// Provides identity-related functionality such as user lookup, role management, token management, etc.
/// </summary>
public class IdentityService
(
    TenantSignInManager signInManager,
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    ITenantResolver tenantResolver,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext context
)
    : IIdentityService
{
    private const string SystemName = "CSharpAPIProject";

    #region User Collection Access

    /// <summary>
    /// Returns the collection of users.
    /// In a derived (global) identity service you can override this to change the source query.
    /// </summary>
    protected virtual IQueryable<ApplicationUser> Users() => userManager.Users;

    public virtual IQueryable<IdentityUser> IdentityUsers() => Users().AsQueryable();

    public virtual IQueryable<ApplicationUser> ApplicationUsers() => Users().AsQueryable();

    #endregion User Collection Access

    #region Private Helpers

    /// <summary>
    /// Retrieves the roles for the specified user.
    /// </summary>
    /// <param name="user">The ApplicationUser instance.</param>
    /// <returns>A list of roles for the user or null if the user is null.</returns>
    private async Task<IList<string>?> GetRolesForUserAsync(ApplicationUser? user)
    {
        if (user == null) return null;
        return await userManager.GetRolesAsync(user);
    }

    /// <summary>
    /// Finds a user by email and optional tenant ID.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="tenantId">Optional tenant ID to filter by.</param>
    /// <returns>The found ApplicationUser or null if not found.</returns>
    public async Task<ApplicationUser?> FindUserByEmailAsync(string email, int? tenantId = null)
    {
        IQueryable<ApplicationUser> Query = tenantId.HasValue
            ? Users().Where(u => u.Email == email && u.TenantId == tenantId.Value)
            : Users().Where(u => u.Email == email);

        return await Query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The found ApplicationUser or null if not found.</returns>
    public async Task<ApplicationUser?> FindUserByIdAsync(string userId, bool entityTracking = true)
    {
        if(entityTracking)
            return await Users().Where(u => u.Id == userId).FirstOrDefaultAsync();
        else
            return await Users().AsNoTracking().Where(u => u.Id == userId).FirstOrDefaultAsync();
    }

    #endregion Private Helpers

    #region Public Helpers
    
    /// <summary>
    /// Checks if a user exists by their unique identifier.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>True if the user exists; otherwise, false.</returns>
    public async Task<bool> UserExists(string userId)
    {
        return await FindUserByIdAsync(userId) != null;
    }

    #endregion Public Helpers

    #region Getters

    /// <summary>
    /// Retrieves the username for a given user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The username if found; otherwise, null.</returns>
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        return user?.UserName;
    }

    /// <summary>
    /// Retrieves the email for a given user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The email if found; otherwise, null.</returns>
    public async Task<string?> GetUserEmailAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        return user?.Email;
    }

    /// <summary>
    /// Retrieves the user ID for a given email and vendor ID.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <returns>The user ID if found; otherwise, null.</returns>
    public async Task<string?> GetUserIdAsync(string email, int vendorId)
    {
        var user = await FindUserByEmailAsync(email, vendorId);
        return user?.Id;
    }

    /// <summary>
    /// Retrieves the user ID for a given phone number and vendor ID.
    /// </summary>
    /// <param name="phoneNumber">The user's phone number.</param>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <returns>The user ID if found; otherwise, null.</returns>
    public async Task<string?> GetUserIdByPhoneNumberAsync(string phoneNumber, int vendorId)
    {
        var user = await Users().FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && u.TenantId == vendorId);
        return user?.Id;
    }

    /// <summary>
    /// Determines whether a user exists with the given email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the user exists; otherwise, false.</returns>
    public async Task<bool> IsUser(string email)
    {
        return await Users().AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves the tenant ID for a given user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The tenant ID if found; otherwise, -1.</returns>
    public async Task<int> GetTenantId(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        return user?.TenantId ?? -1;
    }

    /// <summary>
    /// Retrieves the user ID for a given email using the resolved tenant ID.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user ID if found; otherwise, null.</returns>
    public async Task<string?> GetUserIdAsync(string email)
    {
        var tenantId = tenantResolver.ResolveUserTenantId();
        var user = await FindUserByEmailAsync(email, tenantId);
        return user?.Id;
    }

    /// <summary>
    /// Retrieves the user ID for a given email using the resolved tenant ID.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public string? GetUserId(string email)
    {
        var tenantId = tenantResolver.ResolveUserTenantId();
        return GetUserId(email, tenantId);
    }

    /// <summary>
    /// Retrieves the user ID for a given email and tenant Id
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public string? GetUserId(string email, int TenantId)
    {
        var user = Users().FirstOrDefault(u => u.Email == email && u.TenantId == TenantId);
        return user?.Id;
    }

    /// <summary>
    /// Retrieves a queryable list of users for a vendor based on a search filter and optional role.
    /// </summary>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <param name="searchFilter">The search filter for username or email.</param>
    /// <param name="role">Optional role to filter by.</param>
    /// <returns>A queryable collection of IdentityUser.</returns>
    public async Task<IQueryable<ApplicationUser>> GetUsersForVendor(int vendorId, string searchFilter, string role = "", Guid? designationId = null)
    {
        var userListQuery = Users().Where(u => u.TenantId == vendorId);
        if (!string.IsNullOrEmpty(searchFilter))
        {
            //test if the user is searching with a userID
            bool isUserId = Guid.TryParse(searchFilter, out Guid userId);

            if(isUserId)
                userListQuery = userListQuery.Where(u => u.Id == userId.ToString());
            else
                userListQuery = userListQuery.Where(u => u.UserName.Contains(searchFilter) || u.Email.Contains(searchFilter));
        }

        var userList = await userListQuery.ToListAsync();

        if (!string.IsNullOrEmpty(role))
        {
            // Note: using .Result here is not ideal. Consider refactoring with async/await.
            userList = userList.Where(u => userManager.IsInRoleAsync(u, role).Result).ToList();
        }

        if (designationId.HasValue)
        {
            userList = userList.Where(u => u.DesignationId == designationId).ToList();
        }

        return userList.AsQueryable();
    }

    /// <summary>
    /// Retrieves a queryable list of users based on an array of user IDs.
    /// </summary>
    /// <param name="userIds">An array of user IDs.</param>
    /// <returns>A queryable collection of IdentityUser.</returns>
    public async Task<IQueryable<IdentityUser>> GetUserList(string[] userIds)
    {
        var userListQuery = Users().Where(u => userIds.Contains(u.Id));
        var userList = await userListQuery.ToListAsync();
        return userList.AsQueryable();
    }

    /// <summary>
    /// Retrieves the supplier API user ID for a given supplier ID.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <returns>The API user's ID if found; otherwise, null.</returns>
    public async Task<string?> GetSupplierAPIUser(int supplierId)
    {
        var users = await userManager.GetUsersForClaimAsync(new Claim("SupplierId", supplierId.ToString()));
        return users.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// Retrieves the phone number for a given user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The phone number if found; otherwise, null.</returns>
    public async Task<string?> GetUserPhoneNumberAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        return user?.PhoneNumber;
    }

    #endregion Getters

    #region Create User

    /// <summary>
    /// Creates a new user with the specified username and password.
    /// </summary>
    /// <param name="userName">The username (and email) for the user.</param>
    /// <param name="password">The password for the user.</param>
    /// <returns>A tuple containing the result and the new user's ID.</returns>
    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName, CreationDate = DateTimeOffset.Now };
        var result = await userManager.CreateAsync(user, password);
        return (result.ToApplicationResult(), user.Id);
    }

    /// <summary>
    /// Creates a new user with the specified username, password, and vendor (tenant) ID.
    /// </summary>
    /// <param name="userName">The username (and email) for the user.</param>
    /// <param name="password">The password for the user.</param>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <returns>A tuple containing the result and the new user's ID.</returns>
    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int vendorId)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName, TenantId = vendorId, CreationDate = DateTimeOffset.Now };
        var result = await userManager.CreateAsync(user, password);
        return (result.ToApplicationResult(), user.Id);
    }

    #endregion Create User

    #region Delete User

    /// <summary>
    /// Deletes a user by their user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    /// <summary>
    /// Deletes the specified ApplicationUser.
    /// </summary>
    /// <param name="user">The ApplicationUser instance to delete.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await userManager.DeleteAsync(user);
        return result.ToApplicationResult();
    }

    #endregion Delete User

    #region Claims & Roles
    /// <summary>
    /// Retrieves all roles in the system.
    /// </summary>
    /// <returns></returns>
    public async Task<IList<(string Id, string Name)>> GetAllRolesAsync()
    {
        var roles = await roleManager.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return roles.Select(r => (r.Id, r.Name)).ToList();
    }

    /// <summary>
    /// Retrieves the roles for a user by email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>A list of roles if the user is found; otherwise, null.</returns>
    public async Task<IList<string>?> GetRolesAsync(string email)
    {
        var user = await FindUserByEmailAsync(email);
        return await GetRolesForUserAsync(user);
    }

    /// <summary>
    /// Retrieves the roles for a user by email and vendor ID.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <returns>A list of roles if the user is found; otherwise, null.</returns>
    public async Task<IList<string>?> GetRolesAsync(string email, int vendorId)
    {
        var user = await FindUserByEmailAsync(email, vendorId);
        return await GetRolesForUserAsync(user);
    }

    /// <summary>
    /// Retrieves the roles for a user represented by an IUser interface.
    /// </summary>
    /// <param name="user">The IUser instance.</param>
    /// <returns>A list of roles if the user is found; otherwise, null.</returns>
    public async Task<IList<string>?> GetRolesAsync(IUser user)
    {
        var applicationUser = await FindUserByIdAsync(user.Id);
        return await GetRolesForUserAsync(applicationUser);
    }

    /// <summary>
    /// Retrieves the roles for a user by their Guid identifier.
    /// </summary>
    /// <param name="userId">The user's Guid identifier.</param>
    /// <returns>A list of roles if the user is found; otherwise, null.</returns>
    public async Task<IList<string>?> GetRolesAsync(Guid userId)
    {
        var applicationUser = await FindUserByIdAsync(userId.ToString());
        return await GetRolesForUserAsync(applicationUser);
    }

    /// <summary>
    /// Determines if a user is in a specific role.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user is in the role; otherwise, false.</returns>
    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;
        return await userManager.IsInRoleAsync(user, role);
    }

    /// <summary>
    /// Authorizes a user against a given policy.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="policyName">The name of the policy.</param>
    /// <returns>True if authorization succeeds; otherwise, false.</returns>
    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);
        var result = await authorizationService.AuthorizeAsync(principal, policyName);
        return result.Succeeded;
    }

    /// <summary>
    /// Adds a user to a specified role.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="role">The role to add.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> AddToRoleAsync(string userId, string role)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;
        var result = await userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    /// <summary>
    /// Removes a user from a specified role.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="role">The role to remove.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> RemoveFromRole(string userId, string role)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;
        var result = await userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded;
    }

    /// <summary>
    /// Adds a claim to a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="claim">The claim to add.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> AddClaimAsync(string userId, Claim claim)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;
        var result = await userManager.AddClaimAsync(user, claim);
        return result.Succeeded;
    }

    /// <summary>
    /// Retrieves all claims for a user by their user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>A list of claims if the user is found; otherwise, null.</returns>
    public async Task<IList<Claim>?> GetClaimsAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        var claims = await userManager.GetClaimsAsync(user);

        if (claims == null)
            return new List<Claim>();

        return claims;
    }

    /// <summary>
    /// Retrieves all claims for a user by their Guid identifier.
    /// </summary>
    /// <param name="userId">The user's Guid identifier.</param>
    /// <returns>A list of claims if the user is found; otherwise, null.</returns>
    public async Task<IList<Claim>?> GetClaimsAsync(Guid userId)
    {
        var user = await FindUserByIdAsync(userId.ToString());
        if (user == null) return null;
        return await userManager.GetClaimsAsync(user);
    }

    /// <summary>
    /// Sets a claim for a user. If the claim exists, it is removed and replaced.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="claimName">The claim name.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> SetClaims(string userId, string claimName, string claimValue)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;

        var claims = await userManager.GetClaimsAsync(user);
        var existingClaim = claims.SingleOrDefault(c => c.Type == claimName);
        if (existingClaim != null)
        {
            var removeResult = await userManager.RemoveClaimAsync(user, existingClaim);
            if (removeResult == null || !removeResult.Succeeded)
                return false;
        }

        var result = await userManager.AddClaimAsync(user, new Claim(claimName, claimValue));
        return result.Succeeded;
    }

    #endregion Claims & Roles

    #region Login

    /// <summary>
    /// Attempts to log in a user with the specified email, password, and vendor ID.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="vendorId">The vendor (tenant) ID.</param>
    /// <returns>True if login is successful; otherwise, false.</returns>
    public async Task<bool> LoginUserAsync(string email, string password, int vendorId)
    {
        var user = await Users().SingleOrDefaultAsync(u => u.Email == email && u.TenantId == vendorId);
        if (user == null)
            return false;
        var result = await signInManager.PasswordSignInAsync(user, password, false, false);
        return result?.Succeeded ?? false;
    }

    /// <summary>
    /// Not implemented login method for a user by username and password.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>This method is not implemented and throws a NotImplementedException.</returns>
    public Task<bool> LoginUserAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    #endregion Login

    #region Password Management

    /// <summary>
    /// Resets a user's password based on their email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await FindUserByEmailAsync(email);
        if (user == null)
            return false;

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (removeResult == null || !removeResult.Succeeded)
            return false;

        var addResult = await userManager.AddPasswordAsync(user, newPassword);
        return addResult != null && addResult.Succeeded;
    }

    /// <summary>
    /// Resets a user's password based on their email and a provided token.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await FindUserByEmailAsync(email);
        if (user == null)
            return false;

        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
        return resetResult.Succeeded;
    }

    /// <summary>
    /// Resets a user's password based on their user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> ResetPasswordByIdAsync(string userId, string newPassword)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null)
            return false;

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (removeResult == null || !removeResult.Succeeded)
            return false;

        var addResult = await userManager.AddPasswordAsync(user, newPassword);
        return addResult != null && addResult.Succeeded;
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await FindUserByEmailAsync(email);
        if (user == null)
            return false;
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result?.Succeeded ?? false;
    }

    /// <summary>
    /// Generates a password reset token for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The generated token if successful; otherwise, null.</returns>
    public async Task<string> GeneratePasswordResetToken(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    #endregion Password Management

    #region Change Email

    /// <summary>
    /// Generates an email change token for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="newEmail">The new email address.</param>
    /// <returns>The generated email change token if successful; otherwise, null.</returns>
    public async Task<string> GenerateEmailChangeToken(string userId, string newEmail)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        return await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    /// <summary>
    /// Changes the phone number for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="phoneNumber">The new phone number.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ChangeUserPhoneNumberAsync(string userId, string phoneNumber)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return;
        user.PhoneNumber = phoneNumber;
        await userManager.UpdateAsync(user);
    }

    /// <summary>
    /// Changes the email address for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="newEmail">The new email address.</param>
    /// <param name="changeToken">The email change token.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> ChangeEmailAddressAsync(string userId, string newEmail, string changeToken)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;
        var result = await userManager.ChangeEmailAsync(user, newEmail, changeToken);
        return result.Succeeded;
    }

    #endregion Change Email

    #region API user management

    /// <summary>
    /// Creates a new supplier API user for the specified supplier.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <returns>The new user's ID if successful; otherwise, null.</returns>
    public async Task<string> CreateSupplierApiUserAsync(int supplierId)
    {
        var user = new ApplicationUser();
        var username = Guid.NewGuid().ToString();
        user.UserName = $"{username}@api.ticCSharpAPIProject.com";
        user.Email = $"{username}@api.ticCSharpAPIProject.com";
        var result = await userManager.CreateAsync(user);
        await userManager.AddToRolesAsync(user, new[] { Roles.SupplierApi });
        await userManager.AddClaimAsync(user, new Claim("SupplierId", supplierId.ToString()));
        return result.Succeeded ? user.Id : null;
    }

    /// <summary>
    /// Creates a new vendor API user for the specified vendor.
    /// </summary>
    /// <param name="vendorId">The vendor ID.</param>
    /// <returns>The new user's ID if successful; otherwise, null.</returns>
    public async Task<string> CreateVendorApiUserAsync(int vendorId)
    {
        var user = new ApplicationUser();
        var username = Guid.NewGuid().ToString();
        user.UserName = $"{username}@api.ticCSharpAPIProject.com";
        user.Email = $"{username}@api.ticCSharpAPIProject.com";
        user.TenantId = vendorId;
        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded) { 
            throw new InvalidOperationException($"Failed to create API user for vendor {vendorId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var roleAdded = await userManager.AddToRolesAsync(user, new[] { Roles.VendorApi });

        if(!roleAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException($"Failed to add role VendorApi to user {user.Id} for vendor {vendorId}: {string.Join(", ", roleAdded.Errors.Select(e => e.Description))}");
        }

        var claimAdded = await userManager.AddClaimAsync(user, new Claim("VendorId", vendorId.ToString()));

        if(!claimAdded.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException($"Failed to add claim VendorId to user {user.Id} for vendor {vendorId}: {string.Join(", ", claimAdded.Errors.Select(e => e.Description))}");
        }

        return user.Id;
    }

    /// <summary>
    /// Sets an API key (authentication token) for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The API key token.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> SetApiKeyAsync(string userId, string token)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null)
            return false;
        var saveTokenResult = await userManager.SetAuthenticationTokenAsync(user, "Default", "ApiToken", token);
        return saveTokenResult != null && saveTokenResult.Succeeded;
    }

    /// <summary>
    /// Remove an API key (authentication token) for a user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The API key token.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> RemoveApiKeyAsync(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null)
            return false;
        var saveTokenResult = await userManager.RemoveAuthenticationTokenAsync(user, "Default", "ApiToken");
        return saveTokenResult != null && saveTokenResult.Succeeded;
    }

    /// <summary>
    /// Retrieves the supplier API key for a given supplier.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <returns>The API key if found; otherwise, null.</returns>
    public async Task<string?> GetSupplierApiKeyAsync(int supplierId)
    {
        var usersForClaim = await userManager.GetUsersForClaimAsync(new Claim("SupplierId", supplierId.ToString()));
        var usersInRole = await userManager.GetUsersInRoleAsync(Roles.SupplierApi);
        var user = usersForClaim.Intersect(usersInRole).SingleOrDefault();
        if (user == null) return null;
        return await userManager.GetAuthenticationTokenAsync(user, "Default", "ApiToken");
    }

    /// <summary>
    /// Deletes the supplier API user and removes their API token.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> DeleteSupplierApiUserAndToken(int supplierId)
    {
        // Get the role ID for SupplierApi
        var supplierApiRole = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == Roles.SupplierApi);
        if (supplierApiRole == null) return false;

        // Efficiently find the user using SQL joins - similar to GetVendorApiUserAsync
        var userIds = await (from u in userManager.Users.IgnoreQueryFilters()
                             join userRole in context.Set<IdentityUserRole<string>>() on u.Id equals userRole.UserId
                             join userClaim in context.Set<IdentityUserClaim<string>>() on u.Id equals userClaim.UserId
                             where u.UserName != null
                                 && u.UserName.EndsWith("@api.ticCSharpAPIProject.com")
                                 && userRole.RoleId == supplierApiRole.Id
                                 && userClaim.ClaimType == "SupplierId"
                                 && userClaim.ClaimValue == supplierId.ToString()
                             select u.Id).ToListAsync();

        if (userIds.Count == 0) return false;

        // Delete all API users for this supplier (whether 1 or multiple)
        var deletedCount = 0;
        foreach (var apiUserId in userIds)
        {
            var userToDelete = await FindUserByIdAsync(apiUserId);
            if (userToDelete != null)
            {
                await userManager.RemoveAuthenticationTokenAsync(userToDelete, "Default", "ApiToken");
                var deleteResult = await userManager.DeleteAsync(userToDelete);
                if (deleteResult != null && deleteResult.Succeeded)
                {
                    deletedCount++;
                }
            }
        }
        return deletedCount > 0;
    }

    /// <summary>
    /// Retrieves the vendor API key for a given vendor.
    /// </summary>
    /// <param name="vendorId">The vendor ID.</param>
    /// <returns>The API key if found; otherwise, null.</returns>
    public async Task<string?> GetVendorApiKeyAsync(int vendorId, string? userId = null)
    {
        var usersForClaim = await userManager.Users
            .Where(u => u.TenantId == vendorId && u.UserName.EndsWith("@api.ticCSharpAPIProject.com"))
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        if(usersForClaim.Count > 1)
            usersForClaim = usersForClaim.Where(u => u.Id == userId).ToList();

        if(usersForClaim.Count > 1)
            throw new InvalidOperationException($"{usersForClaim.Count} API users found for vendor {vendorId}. Specify a singular user Id if the vendor has multiple API users.");

        var user = usersForClaim.FirstOrDefault();
        if (user == null) return null;

        if(!await IsInRoleAsync(user.Id, Roles.VendorApi))
            throw new InvalidOperationException($"User {user.Id} is not in the VendorApi role.");

        return await userManager.GetAuthenticationTokenAsync(user, "Default", "ApiToken");
    }

    /// <summary>  
    /// Retrieves the vendor API key for a given vendor and logs the SQL query to the console.  
    /// </summary>  
    /// <param name="vendorId">The vendor ID.</param>  
    /// <returns>The API key if found; otherwise, null.</returns>  
    public async Task<string?> GetVendorApiUserAsync(int vendorId)
    {
        // Get the role ID for VendorApi
        var vendorApiRole = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == Roles.VendorApi);
        if (vendorApiRole == null) return null;

        // Join users to AspNetUserRoles to roles
        var userIds = await (from user in userManager.Users.IgnoreQueryFilters()
                             join userRole in context.Set<IdentityUserRole<string>>() on user.Id equals userRole.UserId
                             where user.TenantId == vendorId
                                 && user.UserName != null
                                 && user.UserName.EndsWith("@api.ticCSharpAPIProject.com")
                                 && userRole.RoleId == vendorApiRole.Id
                             select user.Id).ToListAsync();
        return userIds.FirstOrDefault();
    }

    /// <summary>
    /// Deletes the vendor API user and removes their API token.
    /// </summary>
    /// <param name="vendorId">The vendor ID.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> DeleteVendorApiUserAndToken(int vendorId)
    {
        // Get the role ID for VendorApi
        var vendorApiRole = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == Roles.VendorApi);
        if (vendorApiRole == null) return false;

        // Efficiently find the user using SQL joins - same approach as GetVendorApiUserAsync
        var userIds = await (from u in userManager.Users.IgnoreQueryFilters()
                             join userRole in context.Set<IdentityUserRole<string>>() on u.Id equals userRole.UserId
                             where u.TenantId == vendorId
                                 && u.UserName != null
                                 && u.UserName.EndsWith("@api.ticCSharpAPIProject.com")
                                 && userRole.RoleId == vendorApiRole.Id
                             select u.Id).ToListAsync();

        if (userIds.Count == 0) return false;

        // Delete all API users for this vendor (whether 1 or multiple)
        var deletedCount = 0;
        foreach (var apiUserId in userIds)
        {
            var userToDelete = await FindUserByIdAsync(apiUserId);
            if (userToDelete != null)
            {
                await userManager.RemoveAuthenticationTokenAsync(userToDelete, "Default", "ApiToken");
                var deleteResult = await userManager.DeleteAsync(userToDelete);
                if (deleteResult != null && deleteResult.Succeeded)
                {
                    deletedCount++;
                }
            }
        }
        return deletedCount > 0;
    }

    #endregion API user management

    #region Token Management

    /// <summary>
    /// Generates and saves a refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The generated refresh token if successful; otherwise, null.</returns>
    public async Task<string?> SaveRefreshTokenAsync(string userId, string refreshToken, string expiryTime)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;

        var result = await userManager.SetAuthenticationTokenAsync(user, SystemName, "RefreshToken", refreshToken);
        if (!result.Succeeded)
            return null;

        result = await userManager.SetAuthenticationTokenAsync(user, SystemName, "RefreshTokenExpiry", expiryTime);
        return refreshToken;
    }

    /// <summary>
    /// Retrieves the refresh token for a specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The refresh token if found; otherwise, null.</returns>
    public async Task<string?> GetRefreshToken(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        return await userManager.GetAuthenticationTokenAsync(user, SystemName, "RefreshToken");
    }

    /// <summary>
    /// Retrieves the refresh token expiry for a specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The refresh token expiry if found; otherwise, null.</returns>
    public async Task<string?> GetRefreshTokenExpiry(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        return await userManager.GetAuthenticationTokenAsync(user, SystemName, "RefreshTokenExpiry");
    }

    /// <summary>
    /// Revokes the refresh token for a specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> RevokeRefreshToken(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;

        await userManager.RemoveAuthenticationTokenAsync(user, SystemName, "RefreshToken");
        await userManager.RemoveAuthenticationTokenAsync(user, SystemName, "RefreshTokenExpiry");
        return true;
    }

    /// <summary>
    /// Generates and saves a totp key for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The generated totp key if successful; otherwise, null.</returns>
    public async Task<string?> SaveTotpKeyAsync(string userId, string totpKey)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;

        var result = await userManager.SetAuthenticationTokenAsync(user, SystemName, "TotpKey", totpKey);
        if (!result.Succeeded)
            return null;

        return totpKey;
    }

    /// <summary>
    /// Retrieves the totp key for a specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The refresh token expiry if found; otherwise, null.</returns>
    public async Task<string?> GetTotpKey(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return null;
        return await userManager.GetAuthenticationTokenAsync(user, SystemName, "TotpKey");
    }

    /// <summary>
    /// Revokes the totp key for a specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>True if the operation succeeds; otherwise, false.</returns>
    public async Task<bool> RevokeTotpKey(string userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user == null) return false;

        await userManager.RemoveAuthenticationTokenAsync(user, SystemName, "TotpKey");
        return true;
    }

    #endregion Token Management

    #region Update User

    public async Task<Result> UpdateUserFieldsAsync(string customerEmail, int tenantId, Guid? companyId, Guid? designationId, Guid? teamId, Guid? departmentId, Guid? branchId, Guid? provinceId, Guid? residenceCountryId, Guid? serviceCountryId)
    {
        var user = await FindUserByEmailAsync(customerEmail, tenantId);
        if (user == null)
            return Result.Failure(new[] { $"User with email address {customerEmail} not found." });

        user.CompanyId = companyId == Guid.Empty ? null : companyId;
        user.DesignationId = designationId == Guid.Empty ? null : designationId;
        user.TeamId = teamId == Guid.Empty ? null : teamId;
        user.DepartmentId = departmentId == Guid.Empty ? null : departmentId;
        user.BranchId = branchId == Guid.Empty ? null : branchId;
        user.ProvinceId = provinceId == Guid.Empty ? null : provinceId;
        user.ResidenceCountryId = residenceCountryId == Guid.Empty ? null : residenceCountryId;
        user.ServiceCountryId = serviceCountryId == Guid.Empty ? null : serviceCountryId;

        var result = await userManager.UpdateAsync(user);
        return result.ToApplicationResult();
    }

    #endregion Update User
}
