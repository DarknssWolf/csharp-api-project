using CSharpAPIProject.Domain.Common;
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


        #region Users

        options.AddPolicy(Policies.CanCreate_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanUpdate_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_User,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        #endregion Users

        #region Customer

        options.AddPolicy(Policies.CanCreate_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_Customer,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion Customer

        #region File

        options.AddPolicy(Policies.CanCreate_File,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_File,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanView_File,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion File

        #region Survey

        options.AddPolicy(Policies.CanCreate_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanDelete_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_Survey,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion Survey

        #region Survey Response

        options.AddPolicy(Policies.CanCreate_SurveyResponse,
            policy => policy.RequireRole([
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanView_SurveyResponse,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion Survey Response

        #region Survey Response Set

        options.AddPolicy(Policies.CanCreate_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_SurveyResponseSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion Survey Response Set

        #region UserGroup

        options.AddPolicy(Policies.CanCreate_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanUpdate_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanDelete_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_UserGroup,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        #endregion UserGroup

        #region Vendor

        options.AddPolicy(Policies.CanCreate_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator
            ])
        );

        options.AddPolicy(Policies.CanUpdate_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator
            ])
        );

        options.AddPolicy(Policies.CanView_Vendor,
            policy => policy.RequireRole([
                Roles.Administrator
            ])
        );

        #endregion Vendor

        #region VoteSet

        options.AddPolicy(Policies.CanCreate_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor
            ])
        );

        options.AddPolicy(Policies.CanView_VoteSet,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion VoteSet

        #region VoteItem

        options.AddPolicy(Policies.CanCreate_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanView_VoteItem,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion VoteItem

        #region VoteSubmission

        options.AddPolicy(Policies.CanCreate_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanUpdate_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        options.AddPolicy(Policies.CanDelete_VoteSubmission,
            policy => policy.RequireRole([
                Roles.Administrator,
                Roles.Vendor,
                Roles.Customer
            ])
        );

        #endregion VoteSubmission

        return options;
    }
}
