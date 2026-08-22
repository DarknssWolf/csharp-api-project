using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Domain.Common;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Reflection;
using CSharpAPIProject.Application.Identity;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class SeederHelper(
    IApplicationDbContext context,
    IGlobalIdentityService globalIdentityService,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenGeneratorService tokenGeneratorService)
{

    /// <summary>
    /// Creates a user with the specified credentials and roles
    /// </summary>
    public async Task<ApplicationUser> CreateUserAsync(
        string username,
        string password,
        int vendorId,
        string[] roles,
        Dictionary<string, string> claims,
        bool ignoreFilters = false,
        bool forceClaimUpdate = false)
    {
        // Check if user already exists using GlobalIdentityService
        var existingUserId = await globalIdentityService.GetUserIdAsync(username, vendorId);
        if (!string.IsNullOrEmpty(existingUserId))
        {
            var user = globalIdentityService.IdentityUsers().FirstOrDefault(u => u.Id == existingUserId) as ApplicationUser;
            if (user != null)
            {
                // Log that you cannot create a user that already exists.
                if (forceClaimUpdate)
                    foreach (var claim in claims)
                    {
                        var existingClaims = await globalIdentityService.GetClaimsAsync(existingUserId);
                        if (existingClaims != null && !existingClaims.Any(c => c.Type == claim.Key))
                        {
                            await globalIdentityService.AddClaimAsync(existingUserId, new Claim(claim.Key, claim.Value));
                        }
                    }
                return user;
            }
        }

        // Create new user using GlobalIdentityService
        var (result, userId) = await globalIdentityService.CreateUserAsync(username, password, vendorId);
        
        if (result.Succeeded)
        {
            // Add roles
            foreach (var role in roles)
            {
                await globalIdentityService.AddToRoleAsync(userId, role);
            }
            
            // Add default UserStatus claim
            await globalIdentityService.AddClaimAsync(userId, new Claim("UserStatus", "Active"));
            
            // Add custom claims
            foreach (var claim in claims)
            {
                await globalIdentityService.AddClaimAsync(userId, new Claim(claim.Key, claim.Value));
            }

            // Get the created user
            var createdUser = globalIdentityService.IdentityUsers().FirstOrDefault(u => u.Id == userId) as ApplicationUser;
            return createdUser ?? throw new Exception($"Failed to retrieve created user {username}");
        }
        else
        {
            string errorList = string.Join(System.Environment.NewLine, result.Errors.Select(e => e));
            throw new Exception($"Failed to create user {username}. {errorList}");
        }
    }

    /// <summary>
    /// Creates a supplier API user
    /// </summary>
    public async Task CreateSupplierApiUserAsync(int supplierId)
    {
        await globalIdentityService.CreateSupplierApiUserAsync(supplierId);
    }

    /// <summary>
    /// Creates a vendor API user
    /// </summary>
    public async Task CreateVendorApiUserAsync(int vendorId)
    {
        await globalIdentityService.CreateVendorApiUserAsync(vendorId);
    }

    /// <summary>
    /// Gets a user by username and vendor ID
    /// </summary>
    public string GetUser(string username, int vendorId)
    {
        return globalIdentityService.GetUserId(username, vendorId) ?? string.Empty;
    }

    /// <summary>
    /// Ensures all required roles exist
    /// </summary>
    public async Task EnsureRolesExistAsync()
    {
        var roles = GetAllRoles();
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Gets all available roles from the Roles.cs file
    /// </summary>
    private IEnumerable<string> GetAllRoles()
    {
        return typeof(Roles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => fi.GetRawConstantValue() as string)
            .Where(role => !string.IsNullOrEmpty(role));
    }
} 