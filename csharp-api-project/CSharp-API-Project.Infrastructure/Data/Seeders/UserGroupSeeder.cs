using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class UserGroupSeeder(IApplicationDbContext context)
{
    /// <summary>
    /// Seeds default user groups for the vendor
    /// </summary>
    public async Task SeedDefaultUserGroupsAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        // Check if user groups already exist for this vendor
        if (context.UserGroups.Any(ug => ug.VendorId == vendor.VendorId)) return;

        // Define the user groups to seed
        var userGroupData = new[]
        {
            new { Name = "Administrators", Description = "System administrators with full access", OrderNumber = 1 },
            new { Name = "Managers", Description = "Department and team managers", OrderNumber = 2 },
            new { Name = "Employees", Description = "Regular employees", OrderNumber = 3 },
            new { Name = "Contractors", Description = "External contractors and consultants", OrderNumber = 4 },
            new { Name = "Guests", Description = "Temporary access users", OrderNumber = 5 },
            new { Name = "Support", Description = "Customer support team", OrderNumber = 6 },
            new { Name = "Sales", Description = "Sales and business development team", OrderNumber = 7 },
            new { Name = "Development", Description = "Software development team", OrderNumber = 8 }
        };

        var userGroups = userGroupData.Select(group => new UserGroup
        {
            GroupId = Guid.NewGuid(),
            VendorId = vendor.VendorId,
            Name = group.Name,
            Description = group.Description,
            OrderNumber = group.OrderNumber,
            CreatedBy = backgroundAgentUserId,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow,
            LastModifiedBy = backgroundAgentUserId,
            StatusFlag = new EntityStatus(RecordStatus.Active, backgroundAgentUserId, DateTimeOffset.UtcNow)
        }).ToList();

        await context.UserGroups.AddRangeAsync(userGroups);
        await context.SaveChangesAsync(CancellationToken.None);
    }
} 