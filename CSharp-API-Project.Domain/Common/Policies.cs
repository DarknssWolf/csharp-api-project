namespace CSharpAPIProject.Domain.Common;

public abstract class Policies
{
	/**
	 * PERMISIONS ARE DEFINED IN THE INFRASTRUCTURE LAYER OF THE SOLUTION.
	 * \src\CSharpAPIProject.Infrastructure\Identity\PolicyAuth\PolicyAuth.cs
	 *
	 * Remember that policies do not restrict the data returned from the database.
	 * Only who can access the endpoint to action the data.
	 **/

	#region Users

	public const string CanCreate_User = nameof(CanCreate_User);
	public const string CanUpdate_User = nameof(CanUpdate_User);
	public const string CanDelete_User = nameof(CanDelete_User);
	public const string CanView_User = nameof(CanView_User);

	#endregion Users

	#region Customer

	public const string CanCreate_Customer = nameof(CanCreate_Customer);
	public const string CanUpdate_Customer = nameof(CanUpdate_Customer);
	public const string CanDelete_Customer = nameof(CanDelete_Customer);
	public const string CanView_Customer = nameof(CanView_Customer);

	#endregion Customer

	#region File

	public const string CanCreate_File = nameof(CanCreate_File);
	public const string CanDelete_File = nameof(CanDelete_File);
	public const string CanView_File = nameof(CanView_File);

	#endregion File

	#region Survey

	public const string CanCreate_Survey = nameof(CanCreate_Survey);
	public const string CanUpdate_Survey = nameof(CanUpdate_Survey);
	public const string CanDelete_Survey = nameof(CanDelete_Survey);
	public const string CanView_Survey = nameof(CanView_Survey);

	#endregion Survey

	#region Survey Response

	public const string CanCreate_SurveyResponse = nameof(CanCreate_SurveyResponse);
	public const string CanView_SurveyResponse = nameof(CanView_SurveyResponse);

	#endregion Survey Response

	#region Survey Response Set

	public const string CanCreate_SurveyResponseSet = nameof(CanCreate_SurveyResponseSet);
	public const string CanUpdate_SurveyResponseSet = nameof(CanUpdate_SurveyResponseSet);
	public const string CanDelete_SurveyResponseSet = nameof(CanDelete_SurveyResponseSet);
	public const string CanView_SurveyResponseSet = nameof(CanView_SurveyResponseSet);

	#endregion Survey Response Set

	#region UserGroup

	public const string CanCreate_UserGroup = nameof(CanCreate_UserGroup);
	public const string CanUpdate_UserGroup = nameof(CanUpdate_UserGroup);
	public const string CanDelete_UserGroup = nameof(CanDelete_UserGroup);
	public const string CanView_UserGroup = nameof(CanView_UserGroup);

	#endregion UserGroup

	#region Vendor

	public const string CanCreate_Vendor = nameof(CanCreate_Vendor);
	public const string CanUpdate_Vendor = nameof(CanUpdate_Vendor);
	public const string CanView_Vendor = nameof(CanView_Vendor);

	#endregion Vendor

	#region VoteSet

	public const string CanCreate_VoteSet = nameof(CanCreate_VoteSet);
	public const string CanUpdate_VoteSet = nameof(CanUpdate_VoteSet);
	public const string CanDelete_VoteSet = nameof(CanDelete_VoteSet);
	public const string CanView_VoteSet = nameof(CanView_VoteSet);

	#endregion VoteSet

	#region VoteItem

	public const string CanCreate_VoteItem = nameof(CanCreate_VoteItem);
	public const string CanUpdate_VoteItem = nameof(CanUpdate_VoteItem);
	public const string CanDelete_VoteItem = nameof(CanDelete_VoteItem);
	public const string CanView_VoteItem = nameof(CanView_VoteItem);

	#endregion VoteItem

	#region VoteSubmission

	public const string CanCreate_VoteSubmission = nameof(CanCreate_VoteSubmission);
	public const string CanUpdate_VoteSubmission = nameof(CanUpdate_VoteSubmission);
	public const string CanDelete_VoteSubmission = nameof(CanDelete_VoteSubmission);

	#endregion VoteSubmission
}
