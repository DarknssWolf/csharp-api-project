using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Infrastructure.Identity.Tenant;
using CSharpAPIProject.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Identity;
using CSharpAPIProject.Infrastructure.Data;

/// <summary>
/// Provides identity-related functionality WITHOUT a query filter such as user lookup, role management, token management, etc.
/// </summary>
public class GlobalIdentityService : IdentityService, IGlobalIdentityService
{
    public GlobalIdentityService(
        TenantSignInManager signInManager,
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ITenantResolver tenantResolver,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    ) : base(signInManager, userManager, userClaimsPrincipalFactory, authorizationService, tenantResolver, roleManager, context)
    {
    }

    /// <summary>
    /// Override the base Users() method to ignore tenant-specific query filters.
    /// </summary>
    protected override IQueryable<ApplicationUser> Users()
    {
        return base.Users().IgnoreQueryFilters();
    }
}
