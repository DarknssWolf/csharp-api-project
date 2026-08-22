using System.Security.Claims;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class UserSeeder(IApplicationDbContext context, SeederHelper helper)
{
    public async Task<string> SeedAdministratorUserAsync()
    {
        var claims = new Dictionary<string, string>
        {
            { ClaimTypes.GivenName, "System" },
            { ClaimTypes.Surname, "Administrator" },
            { "UserStatus", "Active" },
        };

        var adminUser = await helper.CreateUserAsync(
            "administrator@local.com", "Administrator1!",
            vendorId: 0,                            // TenantId 0 = the platform/admin tenant
            new[] { Roles.Administrator }, claims);

        return adminUser.Id;
    }
}