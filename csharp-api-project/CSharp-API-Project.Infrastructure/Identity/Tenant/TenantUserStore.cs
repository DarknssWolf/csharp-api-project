using CSharpAPIProject.Application.Identity;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Interfaces.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Identity.Tenant
{
	public class TenantUserStore : UserStore<ApplicationUser>
	{
		private readonly IApplicationDbContext _context;

		public TenantUserStore(IApplicationDbContext context, IdentityErrorDescriber errorDescriber)
			: base((DbContext)context, errorDescriber)
		{
			_context = context;
		}

		public Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, int vendorId, CancellationToken token)
		{
			return Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.TenantId == vendorId, token);
		}
	}
}
