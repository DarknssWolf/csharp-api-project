using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class VendorSeeder(IApplicationDbContext context, SeederHelper helper, string adminUserId)
{
    public async Task SeedDefaultVendorAsync()
    {
        if (context.Vendors.Any(v => v.Email == "vendor@localhost.com")) return;

        var vendor = new Vendor
        {
            Code = Guid.NewGuid().ToString("N")[..12],
            Email = "vendor@localhost.com",
            Name = "Sample Vendor",
            CreatedBy = adminUserId,
            CreatedOn = DateTimeOffset.UtcNow,
        };
        await context.Vendors.AddAsync(vendor);
        await context.SaveChangesAsync(CancellationToken.None); // vendor.VendorId now populated

        // The tenant-admin login for this vendor
        await helper.CreateUserAsync(
            "vendor@localhost.com", "Vendor1!",
            vendor.VendorId,
            new[] { Roles.Vendor },
            new Dictionary<string, string> { { "VendorId", vendor.VendorId.ToString() } });
    }
}