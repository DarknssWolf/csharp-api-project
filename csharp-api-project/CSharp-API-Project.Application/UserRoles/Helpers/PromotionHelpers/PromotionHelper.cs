using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.UserRoles.Helpers.PromotionHelpers;

public static class PromotionHelper
{
	public static List<string> GetValidRolesToGive(List<string> userRoles)
	{
		var promotionPaths = new HashSet<string?>();

		var userIsAdministrator = userRoles.Contains(Roles.Administrator);

		// TIC Staff  
		var userIsTicItAdmin = userRoles.Contains(Roles.TICAdministrator);
		var userIsTicSuperAdmin = userRoles.Contains(Roles.TICSuperAdmin);
		var userIsTicDeveloper = userRoles.Contains(Roles.TICDeveloper);

		// These users don't promote anyone!
		//var userIsTicFinance = roles.Contains(Roles.TICFinance);
		//var userIsTicRewards = roles.Contains(Roles.TICRewards);
		//var userIsTicSupport = roles.Contains(Roles.TICSupport);

		// Client Users  
		var userIsVendor = userRoles.Contains(Roles.Vendor);
		var userIsClientManager = userRoles.Contains(Roles.ClientManager);
		var userIsClientAdmin = userRoles.Contains(Roles.ClientAdmin);

		// These users don't promote anyone!
		//var userIsAccountManager = roles.Contains(Roles.AccountManager);
		//var userIsReportUser = roles.Contains(Roles.ReportUser);
		//var userIsManager = roles.Contains(Roles.Manager);
		//var userIsCustomer = roles.Contains(Roles.Customer);

		var allClientRoles = new[] { Roles.ClientManager, Roles.ClientAdmin, Roles.ReportUser, Roles.Manager, Roles.Customer };

		// Return the Client facing promotion paths.  
		if (userIsClientAdmin) promotionPaths.Add(Roles.ClientAdmin);
		if (userIsClientManager) promotionPaths.UnionWith(allClientRoles);
		if (userIsVendor) promotionPaths.UnionWith(allClientRoles);

		// Return the TIC facing promotion paths.  
		if (userIsTicSuperAdmin)
		{
			promotionPaths.UnionWith(allClientRoles);
			promotionPaths.UnionWith(new[] { Roles.TICSuperAdmin });
		}
		if (userIsTicItAdmin)
		{
			promotionPaths.UnionWith(allClientRoles);
			promotionPaths.UnionWith(new[] { Roles.TICSuperAdmin, Roles.TICAdministrator });
		}
		if (userIsTicDeveloper)
		{
			// TICDevelopers can promote to client roles and other TIC roles
			promotionPaths.UnionWith(allClientRoles);
			promotionPaths.UnionWith(new[] { Roles.TICDeveloper, Roles.TICAdministrator, Roles.TICSuperAdmin });
		}

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
