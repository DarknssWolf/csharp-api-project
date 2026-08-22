
using CSharpAPIProject.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Identity.Tenant
{
	public class TenantUserValidator : IUserValidator<ApplicationUser>
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public TenantUserValidator(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
		{
			var existingUser = await _userManager.Users
				.Where(u => u.Email == user.Email && u.TenantId == user.TenantId)
				.FirstOrDefaultAsync();

			if (existingUser != null)
			{
				return IdentityResult.Failed(new IdentityError
				{
					Code = "DuplicateUser",
					Description = "A user with this email and vendor ID already exists."
				});
			}

			return IdentityResult.Success;
		}
	}
}
