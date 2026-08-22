using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Infrastructure.HelperServices.AuthHelper;
public class AuthHelper
(
    IIdentityService identityService,
    IUser user
) : IAuthHelper
{
    public async Task<bool> LoggedIn_IsVendorAdmin()
    {
        var userRoles = await identityService.GetRolesAsync(user);
        if (userRoles == null) return false;
        return userRoles.Any(r => r == Roles.Vendor);
    }

    public async Task<bool> LoggedIn_IsPlatformAdmin()
    {
        var userRoles = await identityService.GetRolesAsync(user);
        if (userRoles == null) return false;
        return userRoles.Any(r => r == Roles.Administrator);
    }

}
