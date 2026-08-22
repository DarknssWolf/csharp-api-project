using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
namespace CSharpAPIProject.Infrastructure.Data.Seeders;
public class ParticipantSeeder(IApplicationDbContext context, SeederHelper helper)
{
    public async Task SeedDefaultParticipantsAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        for (int i = 1; i <= 3; i++)
        {
            var email = $"customer{i}@localhost.com";
            var customer = new Customer
            {
                VendorId = vendor.VendorId,
                Email = email,
                FirstName = $"Customer{i}",
                LastName = "Sample",
            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync(CancellationToken.None);

            // Each participant gets a login IN THE VENDOR'S TENANT, with a
            // CustomerId claim linking the Identity user to the Customer row.
            await helper.CreateUserAsync(
                email, "Customer1!",
                vendor.VendorId,
                new[] { Roles.Customer },
                new Dictionary<string, string> { { "CustomerId", customer.CustomerId.ToString() } });
        }
    }
}