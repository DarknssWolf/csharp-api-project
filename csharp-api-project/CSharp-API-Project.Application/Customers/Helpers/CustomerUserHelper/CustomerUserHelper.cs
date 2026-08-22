using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Domain.Common;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Customers.Helpers.CustomerUserHelper;

public class CustomerUserHelper(
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IAuthHelper authHelper
)
{
	/// <summary>
	/// Create an ASP Net User for the customer.
	/// </summary>
	/// <param name="Username"></param>
	/// <param name="VendorId"></param>
	/// <param name="CustomerId"></param>
	/// <param name="Password"></param>
	/// <returns></returns>
	public async Task<bool> CreateCustomerUser(string Username, 
		int VendorId, 
		int CustomerId, 
		string Password,
		string FirstName,
		string LastName,
		string Email
	)
	{
        (var result, var userId) = await identityService.CreateUserAsync(Username, Password, VendorId);
        if (!result.Succeeded)
            return false;

        var isTICAdmin = await authHelper.LoggedIn_IsTICUser();
        if (isTICAdmin)
        {
            // Because if you are a TIC admin then we need to ignore query filters
            // otherwise the user of the tenant that you are creating for will not work
            var authorizeResult = await globalIdentityService.AddToRoleAsync(userId, Roles.Customer);

            if (!authorizeResult)
                return false;

            var addCustomerIdClaim = await globalIdentityService.AddClaimAsync(userId, new Claim("CustomerId", CustomerId.ToString()));
            if (!addCustomerIdClaim)
                return false;

            await globalIdentityService.AddClaimAsync(userId, new Claim(ClaimTypes.GivenName, FirstName));
            await globalIdentityService.AddClaimAsync(userId, new Claim(ClaimTypes.Surname, LastName));
            await globalIdentityService.AddClaimAsync(userId, new Claim(ClaimTypes.Email, Email));
        } else
        {
            var authorizeResult = await identityService.AddToRoleAsync(userId, Roles.Customer);

            if (!authorizeResult)
                return false;

            var addCustomerIdClaim = await identityService.AddClaimAsync(userId, new Claim("CustomerId", CustomerId.ToString()));
            if (!addCustomerIdClaim)
                return false;

            await identityService.AddClaimAsync(userId, new Claim(ClaimTypes.GivenName, FirstName));
            await identityService.AddClaimAsync(userId, new Claim(ClaimTypes.Surname, LastName));
            await identityService.AddClaimAsync(userId, new Claim(ClaimTypes.Email, Email));
        }

		

		return true;
	}

}
