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
    public async Task<bool> LoggedIn_IsClientAdmin()
    {
        var userRoles = await identityService.GetRolesAsync(user);
        return userRoles.Any(r => r.Contains(Roles.ClientAdmin) || r.StartsWith(Roles.ClientManager) || r.StartsWith(Roles.Vendor));
    }

    public async Task<bool> LoggedIn_IsTICUser()
    {
        var userRoles = await identityService.GetRolesAsync(user);
        if (userRoles == null) return false;
        var isTICUser = userRoles.Any(r => r.StartsWith("TIC") || r == Roles.Administrator);

        if (!isTICUser)
            isTICUser = userRoles.Any(r => r == Roles.Administrator);

        return isTICUser;
    }

}
