using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Infrastructure.Data;
using CSharpAPIProject.Infrastructure.Data.Interceptors;
using CSharpAPIProject.Infrastructure.Identity;
using System.Security.Claims;
using System.Text;
using CSharpAPIProject.Infrastructure.Identity.Tenant;
using System.Text.Json.Serialization;
using CSharpAPIProject.Infrastructure.HelperServices.SurveyHelperService;
using CSharpAPIProject.Infrastructure.HelperServices.SurveyResponseHelperService;
using CSharpAPIProject.Infrastructure.HelperServices.AuthHelper;
using CSharpAPIProject.Application.Identity;
using CSharpAPIProject.Infrastructure.Identity.PolicyAuth;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;

namespace Microsoft.Extensions.DependencyInjection;

public static class APIDependencyInjection
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		// Get the connection string from the appsetting file
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		if (connectionString == null)
		{
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		}

		// Add EF Core interceptors to the service collection.
		// For more information see https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
		services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>(); // Sets the CreatedBy, Created, LastModifiedBy, and LastModified properties
		services.AddScoped<ITenantResolver, TenantResolver>();

		// Configure the database context
		// Map the IApplicationDbContext interface
		// Add an initializer to the database context
		services.AddDbContext<ApplicationDbContext>(
			(sp, options) =>
			{
				// Map the interceptors to this context
				options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

				// Use SQL Server as the database provider
				options.UseSqlServer(connectionString, o =>
				{
					// Additional SQL Server-specific options can be configured here if needed
				});

			}
		);
		services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
		services.AddScoped<ApplicationDbContextInitialiser>();

		// Add the Identity endpoints to the service collection
		// This wil configure Bearer and Cookie authentication
		// For kore information see https://andrewlock.net/exploring-the-dotnet-8-preview-introducing-the-identity-api-endpoints/
		services.AddIdentityApiEndpoints<ApplicationUser>((options) =>
		{
			options.User.RequireUniqueEmail = true;
		}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

		if (configuration["Jwt:Issuer"] == null)
		{
			throw new InvalidOperationException("Configuration value 'Jwt:Issuer' not found.");
		}

		if (configuration["Jwt:Audience"] == null)
		{
			throw new InvalidOperationException("Configuration value 'Jwt:Audience' not found.");
		}

		if (configuration["Jwt:Key"] == null)
		{
			throw new InvalidOperationException("Configuration value 'Jwt:Key' not found.");
		}

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["Jwt:Issuer"],
				ValidAudience = configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
				NameClaimType = ClaimTypes.Name,
				RoleClaimType = ClaimTypes.Role
			};
		});

		services.AddAuthorization(options =>
		{
			options = PolicyAuth.AddRolesToPolicies(options);
		});


		// Configure other services
		services.AddSingleton(TimeProvider.System);
		
		services.AddTransient<TenantSignInManager>();
		services.AddTransient<IIdentityService, IdentityService>();
		services.AddTransient<IGlobalIdentityService, GlobalIdentityService>();

		services.AddHttpClient();

		services.AddTransient<ISurveyHelperService, SurveyHelperService>();
		services.AddTransient<ISurveyResponseHelperService, SurveyResponseHelperService>();

        services.AddScoped<IAuthHelper, AuthHelper>();

        services.Configure<AspNetCore.Http.Json.JsonOptions>(options =>
		{
			options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
		});

		return services;
	}
}
