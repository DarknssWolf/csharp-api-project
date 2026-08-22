using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CSharpAPIProject.Infrastructure.Data.Seeders;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Interfaces.Entities;

namespace CSharpAPIProject.Infrastructure.Data;

public static class InitialiserExtensions
{
	public static async Task InitialiseDatabaseAsync(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();

		var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

		// Migrations are applied manually with `dotnet ef database update`, not at startup.
		await initialiser.SeedAsync();
	}
}

public class ApplicationDbContextInitialiser(
	ILogger<ApplicationDbContextInitialiser> logger,
	ApplicationDbContext context,
	ITenantResolver tenantResolver,
	IHttpContextAccessor httpContextAccessor,
    IGlobalIdentityService globalIdentityService,
    RoleManager<IdentityRole> roleManager
)
{
    // Seeder instances
    private SeederHelper _seederHelper;
    private UserSeeder _userSeeder;
    private VendorSeeder _vendorSeeder;
    private CustomerSeeder _customerSeeder;
    private SurveySeeder _surveySeeder;
    private UserGroupSeeder _userGroupSeeder;

    /// <summary>
    /// Initialize the seeder instances
    /// </summary>
    private void InitializeSeeders()
    {
        _seederHelper = new SeederHelper(context, globalIdentityService, roleManager);
		_userSeeder = new UserSeeder(context, _seederHelper);
        _vendorSeeder = new VendorSeeder(context, _seederHelper);
        _customerSeeder = new CustomerSeeder(context, _seederHelper);
        _surveySeeder = new SurveySeeder(context);
        _userGroupSeeder = new UserGroupSeeder(context);
    }

	public async Task SeedAsync()
	{
		try
		{
			await TrySeedAsync();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while seeding the database.");
			throw;
		}
	}

	private async Task TrySeedAsync()
	{
		int initialTenantId = tenantResolver.ResolveTenantId();

		// Initialize seeders
		InitializeSeeders();

		// Make sure the roles exist in the database
		await _seederHelper.EnsureRolesExistAsync();

		// Make sure the administrator exists
		var adminUserId = await _userSeeder.SeedAdministratorUserAsync();

		// Default Vendor
		await _vendorSeeder.SeedDefaultVendorAsync(adminUserId);

		// We know that once the vendor is seeded that we need to set the Tenant Id.
		// Otherwise the query filters will determine that the vendor does not exist and attempt to seed the data again.
		var defaultVendor = context.Vendors.Where(v => v.Email == "vendor@localhost.com").FirstOrDefault();
		
		// We will only seed vendor related data if we were able to identify the default vendor
		if(defaultVendor != null)
		{
			tenantResolver.SetTenantId(defaultVendor.VendorId);

			// Customers (with their logins) before surveys and votes, so response seeding has users to work with
			if (!context.Customers.Any()) await _customerSeeder.SeedDefaultCustomersAsync();

			// If Surveys are not loaded, load them
			if (!context.Surveys.Any()) await _surveySeeder.SeedDefaultSurveyDataAsync();

			// A sample vote set with items to vote on
			if (!context.VoteSets.Any()) await new VoteSeeder(context, adminUserId).SeedDefaultVoteDataAsync();

            // If User Groups are not loaded, load them
            if (!context.UserGroups.Any()) await _userGroupSeeder.SeedDefaultUserGroupsAsync(adminUserId);

            // Reset the tenant Id in the Resolver 
            // This way the request will resolve its tenant Id as normal.
            tenantResolver.SetTenantId(initialTenantId);
		}
	}

    private void SetDefaultTenantIdInHeader()
	{
		if (httpContextAccessor.HttpContext == null)
		{
			var context = new DefaultHttpContext();
			context.Request.Headers["X-Tenant-Id"] = "0"; // Set the tenant ID header
			httpContextAccessor.HttpContext = context;
		}
		else
			httpContextAccessor.HttpContext.Request.Headers["X-Tenant-Id"] = "0"; // Set the tenant ID header
	}

	/// <summary>
	/// Gets a User using the username
	/// </summary>
	/// <param name="username"></param>
	/// <returns></returns>
	private async Task<string> GetUserAsync(string username, int vendorId)
	{
        var userId = await globalIdentityService.GetUserIdAsync(username, vendorId);
		if (userId == null) throw new Exception($"User ${username} on Tenant ${vendorId}");
		else return userId;
		
	}

}
