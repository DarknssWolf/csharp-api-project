using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace CSharpAPIProject.Infrastructure.Identity.PolicyAuth;

public static class PolicyAuth
{
    public static AuthorizationOptions AddRolesToPolicies(AuthorizationOptions options)
    {
        /**
		 * POLICIES ARE DEFINED IN THE DOMAIN LAYER IN THE `Policies.cs` FILE
		 * \src\CSharpAPIProject.Domain\Common\Policies.cs
		 * 
		 * Remember that policies do not restrict the data returned from the database.
		 * Only who can access the endpoint to action the data.
		 **/

       
        #region Customer

        options.AddPolicy(Policies.CanCreate_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager
            ])
        );

        options.AddPolicy(Policies.CanView_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.VendorApi,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Customer

        #region Delegates

        options.AddPolicy(Policies.CanCreate_Delegate,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Delegate,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_Delegate,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanView_Delegate,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Delegates

        #region Survey

        options.AddPolicy(Policies.CanCreate_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanDelete_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Survey

        #region Survey Response

        options.AddPolicy(Policies.CanCreate_SurveyResponse,
            policy => policy.RequireRole([
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_SurveyResponse,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_SurveyResponse,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Survey Response

        #region Survey Response Set

        options.AddPolicy(Policies.CanCreate_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Survey Response Set

        #region Vote set

        options.AddPolicy(Policies.CanCreate_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Vote set

        #region Vote Item

        options.AddPolicy(Policies.CanCreate_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanView_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Vote Item

        #region Vote Submission

        options.AddPolicy(Policies.CanCreate_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        options.AddPolicy(Policies.CanView_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer,
                Roles.ReportUser
            ])
        );

        #endregion Vote Submission

        #region Users

        options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));

        options.AddPolicy(Policies.CanCreate_TIC_IT_AdminUser,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator
            ])
        );

        options.AddPolicy(Policies.CanCreate_Client_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager
            ])
        );

        options.AddPolicy(Policies.CanDelete_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager
            ])
        );

        options.AddPolicy(Policies.CanUpdate_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_UserStatus,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager
            ])
        );

        options.AddPolicy(Policies.CanView_UserListByVendor,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager
            ])
        );

        options.AddPolicy(Policies.CanImpersonate_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.VendorApi // So that the token exchange can happen.
            ])
        );

        options.AddPolicy(Policies.CanView_Impersonation,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.Customer
            ])
        );

        #endregion Users

        #region Vendors

        options.AddPolicy(Policies.CanReset_VendorApiKey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICDeveloper
            ])
        );

        options.AddPolicy(Policies.CanDelete_VendorApiUser,
            policy => policy.RequireRole([
                Roles.TICAdministrator,
                Roles.Administrator,
            ])
        );

        options.AddPolicy(Policies.CanCreate_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanDelete_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        #endregion Vendors

        #region Vendor Language

        options.AddPolicy(Policies.CanCreate_VendorLanguage,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VendorLanguage,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanDelete_VendorLanguage,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_VendorLanguage,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin
            ])
        );

        #endregion Vendor Language

        #region User Groups

        options.AddPolicy(Policies.CanCreate_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanUpdate_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.ClientManager,
                Roles.ClientAdmin
            ])
        );

        options.AddPolicy(Policies.CanView_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.Vendor,
                Roles.VendorApi,
                Roles.ClientManager,
                Roles.ClientAdmin,
                Roles.ReportUser
            ])
        );

        options.AddPolicy(Policies.CanDelete_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.TICAdministrator,
                Roles.TICSuperAdmin,
                Roles.ClientAdmin
            ])
        );

        #endregion User Groups

        return options;
    }
}
