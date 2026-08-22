using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.UserRoles.Helpers.PromotionHelpers;

public static class PromotionHelper
{
	public static List<string> GetValidRolesToGive(List<string> userRoles)
	{
		var promotionPaths = new HashSet<string?>();

		var userIsAdministrator = userRoles.Contains(Roles.Administrator);

		// Vendor Users
		var userIsVendor = userRoles.Contains(Roles.Vendor);

		// These users don't promote anyone!
		//var userIsCustomer = roles.Contains(Roles.Customer);

		var allVendorRoles = new[] { Roles.Vendor, Roles.Customer };

		// Return the Vendor facing promotion paths.
		if (userIsVendor) promotionPaths.UnionWith(allVendorRoles);

		// Return promotion paths for full system administrators.
		if (userIsAdministrator)
		{
			var allRoles = typeof(Roles).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
										.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
										.Select(fi => fi.GetRawConstantValue() as string);
			promotionPaths.UnionWith(allRoles);
		}

		return promotionPaths.ToList();
	}
}
