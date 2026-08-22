using Microsoft.AspNetCore.Identity;

namespace CSharpAPIProject.Application.Interfaces.Services;
public interface IGlobalIdentityService : IIdentityService
{
	IQueryable<IdentityUser> IdentityUsers();
}
