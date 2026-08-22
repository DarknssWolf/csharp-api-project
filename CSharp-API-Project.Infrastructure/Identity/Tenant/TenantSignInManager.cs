using CSharpAPIProject.Application.Identity;
using CSharpAPIProject.Application.Interfaces.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharpAPIProject.Infrastructure.Identity.Tenant
{
	public class TenantSignInManager : SignInManager<ApplicationUser>
    {
		private ITenantResolver _tenantResolver;

        public TenantSignInManager(UserManager<ApplicationUser> userManager, 
			IHttpContextAccessor contextAccessor, 
			IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, 
			IOptions<IdentityOptions> optionsAccessor, 
			ILogger<SignInManager<ApplicationUser>> logger, 
			IAuthenticationSchemeProvider schemes, 
			IUserConfirmation<ApplicationUser> confirmation,
			ITenantResolver tenantResolver
		) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
			_tenantResolver = tenantResolver;
		}

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent)
        {
			var tenantId = _tenantResolver.ResolveUserTenantId();
			var user = await UserManager.FindByEmailAsync(email);
            if (user == null || user.TenantId != tenantId)
                return SignInResult.Failed;

            return await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure: false);
        }
    }
}
