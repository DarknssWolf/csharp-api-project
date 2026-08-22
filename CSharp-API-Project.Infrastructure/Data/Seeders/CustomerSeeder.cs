using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using System.Text.Json;
using CSharpAPIProject.Domain.ValueObjects;
using System.Dynamic;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class CustomerSeeder(
    IApplicationDbContext context, 
    SeederHelper helper
)
{
    public async Task SeedDefaultCustomersAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        var region = context.Regions.FirstOrDefault(r => r.VendorId == vendor.VendorId);
        if (region == null) return;

        for (int i = 1; i <= 5; i++)
        {
            var customerAddress = new Address(
                $"Test Street {i}",
                $"test suburb {i}",
                $"test city {i}",
                $"test province {i}",
                $"test country {i}",
                $"1234{i}");

            dynamic customerDataObj = new ExpandoObject();
            customerDataObj.wants_email_spam = i % 2 == 0;
            customerDataObj.wants_sms_spam = i % 2 != 0;
            customerDataObj.otherCustomerData = $"Other data for customer {i}";
            string customerDataJson = JsonSerializer.Serialize(customerDataObj, new JsonSerializerOptions { WriteIndented = true });

            var email = $"customer{i}@localhost.com";
            var customer = new Customer
            {
                VendorId = vendor.VendorId,
                VendorRegionId = region.RegionId,
                Email = email,
                FirstName = $"Customer{i}",
                LastName = $"One{i}",
                Phone = $"123456789{i}",
                Address = customerAddress,
                IDNumber = $"12345678{i}",
                CustomerData = customerDataJson,
                CreatedBy = helper.GetUser("administrator@local.com", 0),
                CreatedOn = DateTimeOffset.UtcNow,
            };

            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync(CancellationToken.None);

            var claims = new Dictionary<string, string>
            {
                { "CustomerId", customer.CustomerId.ToString() }
            };

            await helper.CreateUserAsync(
                email,
                "Customer1!",
                vendor.VendorId,
                new[] { Roles.Customer },
                claims,
                false,
                true);

        }
    }
  
}