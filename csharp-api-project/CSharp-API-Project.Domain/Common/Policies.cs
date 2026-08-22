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

	public const string CanPurge = nameof(CanPurge);

	#region Users

	public const string CanCreate_TIC_IT_AdminUser = nameof(CanCreate_TIC_IT_AdminUser);
	public const string CanCreate_Client_User = nameof(CanCreate_Client_User);
	public const string CanUpdate_User = nameof(CanUpdate_User);
    public const string CanUpdate_UserStatus = nameof(CanUpdate_UserStatus);
    public const string CanDelete_User = nameof(CanDelete_User);
	public const string CanView_UserListByVendor = nameof(CanView_UserListByVendor);
	public const string CanImpersonate_User = nameof(CanImpersonate_User);
	public const string CanView_Impersonation = nameof(CanView_Impersonation);

	#endregion Users

	#region Calender Entry Tag

	public const string CanCreate_CalendarEntryTag = nameof(CanCreate_CalendarEntryTag);
	public const string CanUpdate_CalendarEntryTag = nameof(CanUpdate_CalendarEntryTag);
	public const string CanDelete_CalendarEntryTag = nameof(CanDelete_CalendarEntryTag);
	public const string CanView_CalendarEntryTag = nameof(CanView_CalendarEntryTag);

	#endregion Calender Entry Tag

	#region Calender
	
	public const string CanCreate_Calendar = nameof(CanCreate_Calendar);
	public const string CanUpdate_Calendar = nameof(CanUpdate_Calendar);
	public const string CanDelete_Calendar = nameof(CanDelete_Calendar);
	public const string CanView_Calendar = nameof(CanView_Calendar);

	#endregion Calender

	#region Calender Entry

	public const string CanCreate_CalendarEntry = nameof(CanCreate_CalendarEntry);
	public const string CanUpdate_CalendarEntry = nameof(CanUpdate_CalendarEntry);
	public const string CanDelete_CalendarEntry = nameof(CanDelete_CalendarEntry);
	public const string CanView_CalendarEntry = nameof(CanView_CalendarEntry);

	public const string CanView_CalendarEntryByDateRange = nameof(CanView_CalendarEntryByDateRange);

    #endregion Calender Entry

    #region Category

    public const string CanCreate_Category = nameof(CanCreate_Category);
    public const string CanUpdate_Category = nameof(CanUpdate_Category);
    public const string CanDelete_Category = nameof(CanDelete_Category);
    public const string CanView_Category = nameof(CanView_Category);

    #endregion Category

    #region Campaign

    public const string CanCreate_Campaign = nameof(CanCreate_Campaign);
    public const string CanUpdate_Campaign = nameof(CanUpdate_Campaign);
    public const string CanDelete_Campaign = nameof(CanDelete_Campaign);
    public const string CanView_Campaign = nameof(CanView_Campaign);

    #endregion Campaign

    #region CampaignDelegate

    public const string CanCreate_CampaignDelegate = nameof(CanCreate_CampaignDelegate);
    public const string CanUpdate_CampaignDelegate = nameof(CanUpdate_CampaignDelegate);
    public const string CanDelete_CampaignDelegate = nameof(CanDelete_CampaignDelegate);
    public const string CanView_CampaignDelegate = nameof(CanView_CampaignDelegate);

    #endregion CampaignDelegate

    #region CampaignUploadCategory

    public const string CanCreate_CampaignUploadCategory = nameof(CanCreate_CampaignUploadCategory);
    public const string CanUpdate_CampaignUploadCategory = nameof(CanUpdate_CampaignUploadCategory);
    public const string CanDelete_CampaignUploadCategory = nameof(CanDelete_CampaignUploadCategory);
    public const string CanView_CampaignUploadCategory = nameof(CanView_CampaignUploadCategory);

    #endregion CampaignUploadCategory

    #region CompanyTier

    public const string CanCreate_CompanyTier = nameof(CanCreate_CompanyTier);
    public const string CanUpdate_CompanyTier = nameof(CanUpdate_CompanyTier);
    public const string CanDelete_CompanyTier = nameof(CanDelete_CompanyTier);
    public const string CanView_CompanyTier = nameof(CanView_CompanyTier);

    #endregion CompanyTier

    #region CompanyKind

    public const string CanCreate_CompanyKind = nameof(CanCreate_CompanyKind);
    public const string CanUpdate_CompanyKind = nameof(CanUpdate_CompanyKind);
    public const string CanDelete_CompanyKind = nameof(CanDelete_CompanyKind);
    public const string CanView_CompanyKind = nameof(CanView_CompanyKind);

    #endregion CompanyKind

    #region Country

    public const string CanCreate_Country = nameof(CanCreate_Country);
    public const string CanUpdate_Country = nameof(CanUpdate_Country);
    public const string CanDelete_Country = nameof(CanDelete_Country);
    public const string CanView_Country = nameof(CanView_Country);

    #endregion Country

    #region Province

    public const string CanCreate_Province = nameof(CanCreate_Province);
    public const string CanUpdate_Province = nameof(CanUpdate_Province);
    public const string CanDelete_Province = nameof(CanDelete_Province);
    public const string CanView_Province = nameof(CanView_Province);

    #endregion Province

    #region SubRegion

    public const string CanCreate_SubRegion = nameof(CanCreate_SubRegion);
    public const string CanUpdate_SubRegion = nameof(CanUpdate_SubRegion);
    public const string CanDelete_SubRegion = nameof(CanDelete_SubRegion);
    public const string CanView_SubRegion = nameof(CanView_SubRegion);

    #endregion SubRegion

    #region Department

    public const string CanCreate_Department = nameof(CanCreate_Department);
    public const string CanUpdate_Department = nameof(CanUpdate_Department);
    public const string CanDelete_Department = nameof(CanDelete_Department);
    public const string CanView_Department = nameof(CanView_Department);

    #endregion Department

    #region Designation

    public const string CanCreate_Designation = nameof(CanCreate_Designation);
    public const string CanUpdate_Designation = nameof(CanUpdate_Designation);
    public const string CanDelete_Designation = nameof(CanDelete_Designation);
    public const string CanView_Designation = nameof(CanView_Designation);

    #endregion Designation

    #region Team

    public const string CanCreate_Team = nameof(CanCreate_Team);
    public const string CanUpdate_Team = nameof(CanUpdate_Team);
    public const string CanDelete_Team = nameof(CanDelete_Team);
    public const string CanView_Team = nameof(CanView_Team);

    #endregion Team

    #region Branch

    public const string CanCreate_Branch = nameof(CanCreate_Branch);
    public const string CanUpdate_Branch = nameof(CanUpdate_Branch);
    public const string CanDelete_Branch = nameof(CanDelete_Branch);
    public const string CanView_Branch = nameof(CanView_Branch);

    #endregion Branch

    #region Claim Period

    public const string CanCreate_ClaimPeriod = nameof(CanCreate_ClaimPeriod);
    public const string CanUpdate_ClaimPeriod = nameof(CanUpdate_ClaimPeriod);
    public const string CanDelete_ClaimPeriod = nameof(CanDelete_ClaimPeriod);
    public const string CanView_ClaimPeriod = nameof(CanView_ClaimPeriod);

    #endregion Claim Period

    #region Customer

    public const string CanCreate_Customer = nameof(CanCreate_Customer);
	public const string CanUpdate_Customer = nameof(CanUpdate_Customer);
	public const string CanDelete_Customer = nameof(CanDelete_Customer);
	public const string CanView_Customer = nameof(CanView_Customer);

    #endregion Customer

    #region Customer Ledger

    public const string CanCredit_CustomerLedger = nameof(CanCredit_CustomerLedger);
    public const string CanDebit_CustomerLedger = nameof(CanDebit_CustomerLedger);
    public const string CanView_CustomerLedger = nameof(CanView_CustomerLedger);
    public const string CanQuery_OwnLedger = nameof(CanQuery_OwnLedger);

    #endregion Customer Ledger

    #region Delegate

    public const string CanCreate_Delegate = nameof(CanCreate_Delegate);
	public const string CanUpdate_Delegate = nameof(CanUpdate_Delegate);
	public const string CanDelete_Delegate = nameof(CanDelete_Delegate);
	public const string CanView_Delegate = nameof(CanView_Delegate);

	#endregion Delegate

	#region Exchange Rate

	public const string CanSet_ExchangeRate = nameof(CanSet_ExchangeRate);
	public const string CanView_ExchangeRate = nameof(CanView_ExchangeRate);

    #endregion Exchange Rate

    #region Finance Entry

    public const string CanCreate_FinanceEntry = nameof(CanCreate_FinanceEntry);
    public const string CanUpdate_FinanceEntry = nameof(CanUpdate_FinanceEntry);
    public const string CanDelete_FinanceEntry = nameof(CanDelete_FinanceEntry);
    public const string CanView_FinanceEntry = nameof(CanView_FinanceEntry);

    #endregion Finance Entry

    #region Invoice

    public const string CanCreate_Invoice = nameof(CanCreate_Invoice);
    public const string CanUpdate_Invoice = nameof(CanUpdate_Invoice);
    public const string CanDelete_Invoice = nameof(CanDelete_Invoice);
    public const string CanView_Invoice = nameof(CanView_Invoice);

    #endregion Invoice

    #region Finance Entry Item

    public const string CanCreate_FinanceEntryItem = nameof(CanCreate_FinanceEntryItem);
    public const string CanUpdate_FinanceEntryItem = nameof(CanUpdate_FinanceEntryItem);
    public const string CanDelete_FinanceEntryItem = nameof(CanDelete_FinanceEntryItem);
    public const string CanView_FinanceEntryItem = nameof(CanView_FinanceEntryItem);

    #endregion Finance Entry Item

    #region Gallery

    public const string CanCreate_Gallery = nameof(CanCreate_Gallery);
	public const string CanUpdate_Gallery = nameof(CanUpdate_Gallery);
	public const string CanDelete_Gallery = nameof(CanDelete_Gallery);
	public const string CanView_Gallery = nameof(CanView_Gallery);

	#endregion Gallery

	#region Gallery Image

	public const string CanCreate_GalleryImage = nameof(CanCreate_GalleryImage);
	public const string CanUpdate_GalleryImage = nameof(CanUpdate_GalleryImage);
	public const string CanDelete_GalleryImage = nameof(CanDelete_GalleryImage);
	public const string CanView_GalleryImage = nameof(CanView_GalleryImage);

	public const string CanUpdate_GalleryImageStatus = nameof(CanUpdate_GalleryImageStatus);
	public const string CanGet_GalleryImageUploadLink = nameof(CanGet_GalleryImageUploadLink);

    #endregion Gallery Image

    #region Pending Points

    public const string CanCreate_PendingPoints = nameof(CanCreate_PendingPoints);
    public const string CanUpdate_PendingPoints = nameof(CanUpdate_PendingPoints);
    public const string CanDelete_PendingPoints = nameof(CanDelete_PendingPoints);
    public const string CanView_PendingPoints = nameof(CanView_PendingPoints);

    #endregion Pending Points

    #region Point File Upload

    public const string CanCreate_PointFileUpload = nameof(CanCreate_PointFileUpload);
    public const string CanUpdate_PointFileUpload = nameof(CanUpdate_PointFileUpload);
    public const string CanDelete_PointFileUpload = nameof(CanDelete_PointFileUpload);
    public const string CanView_PointFileUpload = nameof(CanView_PointFileUpload);

    #endregion Point File Upload

    #region Price List

    public const string CanCreate_PriceList = nameof(CanCreate_PriceList);
	public const string CanUpdate_PriceList = nameof(CanUpdate_PriceList);
	public const string CanDelete_PriceList = nameof(CanDelete_PriceList);
	public const string CanView_PriceList = nameof(CanView_PriceList);

	#endregion Price List

	#region Products

	public const string CanCreate_Product = nameof(CanCreate_Product);
	public const string CanUpdate_Product = nameof(CanUpdate_Product);
	public const string CanDelete_Product = nameof(CanDelete_Product);
	public const string CanView_Product = nameof(CanView_Product);

	#endregion Products

	#region Product Orders

	public const string CanCreate_ProductOrder = nameof(CanCreate_ProductOrder);
	public const string CanUpdate_ProductOrder = nameof(CanUpdate_ProductOrder);
	public const string CanDelete_ProductOrder = nameof(CanDelete_ProductOrder);
	public const string CanView_ProductOrder = nameof(CanView_ProductOrder);

	#endregion Product Orders

    #region Poll

    public const string CanCreate_Poll = nameof(CanCreate_Poll);
	public const string CanUpdate_Poll = nameof(CanUpdate_Poll);
	public const string CanDelete_Poll = nameof(CanDelete_Poll);
	public const string CanView_Poll = nameof(CanView_Poll);

	#endregion Poll

	#region Poll Response

	public const string CanCreate_PollResponse = nameof(CanCreate_PollResponse);
	public const string CanUpdate_PollResponse = nameof(CanUpdate_PollResponse);
	public const string CanView_PollResponse = nameof(CanView_PollResponse);

    #endregion Poll Response

    #region Quiz

    public const string CanCreate_Quiz = nameof(CanCreate_Quiz);
    public const string CanUpdate_Quiz = nameof(CanUpdate_Quiz);
    public const string CanDelete_Quiz = nameof(CanDelete_Quiz);
    public const string CanView_Quiz = nameof(CanView_Quiz);

    #endregion Quiz

    #region Quiz Response

    public const string CanCreate_QuizResponse = nameof(CanCreate_QuizResponse);
    public const string CanUpdate_QuizResponse = nameof(CanUpdate_QuizResponse);
    public const string CanView_QuizResponse = nameof(CanView_QuizResponse);

    #endregion Quiz Response

    #region Quiz Response Set

    public const string CanCreate_QuizResponseSet = nameof(CanCreate_QuizResponseSet);
    public const string CanUpdate_QuizResponseSet = nameof(CanUpdate_QuizResponseSet);
    public const string CanDelete_QuizResponseSet = nameof(CanDelete_QuizResponseSet);
    public const string CanView_QuizResponseSet = nameof(CanView_QuizResponseSet);

    #endregion Quiz Response Set

    #region Supplier

    public const string CanReset_SupplierApiKey = nameof(CanReset_SupplierApiKey);
    public const string CanCreate_Supplier = nameof(CanCreate_Supplier);
    public const string CanUpdate_Supplier = nameof(CanUpdate_Supplier);
    public const string CanDelete_Supplier = nameof(CanDelete_Supplier);
    public const string CanView_Supplier = nameof(CanView_Supplier);
    public const string CanDelete_SupplierApiUser = nameof(CanDelete_SupplierApiUser);

    #endregion Supplier

    #region Supplier Ledger

    public const string CanCredit_SupplierLedger = nameof(CanCredit_SupplierLedger);
    public const string CanDebit_SupplierLedger = nameof(CanDebit_SupplierLedger);
    public const string CanView_SupplierLedger = nameof(CanView_SupplierLedger);

    #endregion Supplier Ledger

    #region Survey

    public const string CanCreate_Survey = nameof(CanCreate_Survey);
    public const string CanUpdate_Survey = nameof(CanUpdate_Survey);
    public const string CanDelete_Survey = nameof(CanDelete_Survey);
    public const string CanView_Survey = nameof(CanView_Survey);

    #endregion Survey

    #region Survey Response

    public const string CanCreate_SurveyResponse = nameof(CanCreate_SurveyResponse);
    public const string CanUpdate_SurveyResponse = nameof(CanUpdate_SurveyResponse);
    public const string CanView_SurveyResponse = nameof(CanView_SurveyResponse);

    #endregion Survey Response

    #region Survey Response Set

    public const string CanCreate_SurveyResponseSet = nameof(CanCreate_SurveyResponseSet);
    public const string CanUpdate_SurveyResponseSet = nameof(CanUpdate_SurveyResponseSet);
    public const string CanDelete_SurveyResponseSet = nameof(CanDelete_SurveyResponseSet);
    public const string CanView_SurveyResponseSet = nameof(CanView_SurveyResponseSet);

    #endregion Survey Response Set

    #region Statistics

    public const string CanView_StatisticEntityTotals = nameof(CanView_StatisticEntityTotals);

    #endregion Statistics

    #region Settings

    // WARNING: THIS HAS NO EFFECT ON THE SETTINGS SERVICES. THIS ONLY AFFECTS THE ENDPOINTS
    public const string CanSet_TICSetting = nameof(CanSet_TICSetting);
    public const string CanView_TICSetting = nameof(CanView_TICSetting);

    #endregion Settings

    #region VoteSet

    public const string CanCreate_VoteSet = nameof(CanCreate_VoteSet);
	public const string CanUpdate_VoteSet = nameof(CanUpdate_VoteSet);
	public const string CanDelete_VoteSet = nameof(CanDelete_VoteSet);
	public const string CanView_VoteSet = nameof(CanView_VoteSet);

	#endregion VoteSet

	#region Vote Submission

	public const string CanCreate_VoteSubmission = nameof(CanCreate_VoteSubmission);
	public const string CanDelete_VoteSubmission = nameof(CanDelete_VoteSubmission);
	public const string CanUpdate_VoteSubmission = nameof(CanUpdate_VoteSubmission);
	public const string CanView_VoteSubmission = nameof(CanView_VoteSubmission);

	#endregion Vote Submission

	#region Vote Item

	public const string CanCreate_VoteItem = nameof(CanCreate_VoteItem);
	public const string CanDelete_VoteItem = nameof(CanDelete_VoteItem);
	public const string CanUpdate_VoteItem = nameof(CanUpdate_VoteItem);
	public const string CanView_VoteItem = nameof(CanView_VoteItem);

	#endregion Vote Item

	#region Vendor

	public const string CanReset_VendorApiKey = nameof(CanReset_VendorApiKey);    
    public const string CanCreate_Vendor = nameof(CanCreate_Vendor);
	public const string CanUpdate_Vendor = nameof(CanUpdate_Vendor);
	public const string CanDelete_Vendor = nameof(CanDelete_Vendor);
	public const string CanView_Vendor = nameof(CanView_Vendor);
    public const string CanDelete_VendorApiUser = nameof(CanDelete_VendorApiUser);

    #endregion Vendor

    #region Vendor Language

    public const string CanCreate_VendorLanguage = nameof(CanCreate_VendorLanguage);
    public const string CanUpdate_VendorLanguage = nameof(CanUpdate_VendorLanguage);
    public const string CanDelete_VendorLanguage = nameof(CanDelete_VendorLanguage);
    public const string CanView_VendorLanguage = nameof(CanView_VendorLanguage);

    #endregion Vendor Language

    #region Vendor Ledger

    public const string CanCredit_VendorLedger = nameof(CanCredit_VendorLedger);
    public const string CanDebit_VendorLedger = nameof(CanDebit_VendorLedger);
    public const string CanView_VendorLedger = nameof(CanView_VendorLedger);

    #endregion Vendor Ledger

    #region Vendor Region

    public const string CanAdd_VendorToRegion = nameof(CanAdd_VendorToRegion);
	public const string CanRemove_VendorFromRegion = nameof(CanRemove_VendorFromRegion);
	public const string CanView_VendorRegions = nameof(CanView_VendorRegions);

    #endregion Vendor Region

    #region Vendor Settings

    // WARNING: THIS HAS NO EFFECT ON THE SETTINGS SERVICES. THIS ONLY AFFECTS THE ENDPOINTS
    public const string CanSet_VendorSetting = nameof(CanSet_VendorSetting);
    public const string CanView_VendorSetting = nameof(CanView_VendorSetting);

    #endregion Vendor Settings

    #region Mailer

    public const string CanCreate_GenericEmail = nameof(CanCreate_GenericEmail);

    #endregion Mailer

    #region Content Pages

    public const string CanCreate_ContentPage = nameof(CanCreate_ContentPage);
    public const string CanUpdate_ContentPage = nameof(CanUpdate_ContentPage);
    public const string CanView_ContentPage = nameof(CanView_ContentPage);
    public const string CanView_ContentPageList = nameof(CanView_ContentPageList);

    #endregion VoteSet

    #region Nomination

    public const string CanCreate_Nomination = nameof(CanCreate_Nomination);
    public const string CanUpdate_Nomination = nameof(CanUpdate_Nomination);
    public const string CanDelete_Nomination = nameof(CanDelete_Nomination);
    public const string CanView_Nomination = nameof(CanView_Nomination);

    #endregion Nomination

    #region Nomination Submissions

    public const string CanCreate_NominationSubmission = nameof(CanCreate_NominationSubmission);
    public const string CanUpdate_NominationSubmission = nameof(CanUpdate_NominationSubmission);
    public const string CanDelete_NominationSubmission = nameof(CanDelete_NominationSubmission);
    public const string CanView_NominationSubmission = nameof(CanView_NominationSubmission);

    #endregion Nomination Submissions

    #region Nomination Approvals

    public const string CanCreate_NominationApprovals = nameof(CanCreate_NominationApprovals);
    public const string CanUpdate_NominationApprovals = nameof(CanUpdate_NominationApprovals);
    public const string CanDelete_NominationApprovals = nameof(CanDelete_NominationApprovals);
    public const string CanView_NominationApprovals = nameof(CanView_NominationApprovals);

    #endregion Nomination Approvals

    #region UserGroups

    public const string CanCreate_UserGroup = nameof(CanCreate_UserGroup);
    public const string CanUpdate_UserGroup = nameof(CanUpdate_UserGroup);
    public const string CanDelete_UserGroup = nameof(CanDelete_UserGroup);
    public const string CanView_UserGroup = nameof(CanView_UserGroup);

    #endregion UserGroups

    #region Visibility Configurations

    public const string CanCreate_VisibilityConfiguration = nameof(CanCreate_VisibilityConfiguration);
    public const string CanUpdate_VisibilityConfiguration = nameof(CanUpdate_VisibilityConfiguration);
    public const string CanDelete_VisibilityConfiguration = nameof(CanDelete_VisibilityConfiguration);
    public const string CanView_VisibilityConfiguration = nameof(CanView_VisibilityConfiguration);

    #endregion UserGroups

    #region Translations

    public const string CanCreate_Translations = nameof(CanCreate_Translations);
    public const string CanUpdate_Translations = nameof(CanUpdate_Translations);
    public const string CanView_Translations = nameof(CanView_Translations);

    #endregion Translations

    #region Tasks

    public const string CanCreate_Task = nameof(CanCreate_Task);
    public const string CanUpdate_Task = nameof(CanUpdate_Task);
    public const string CanDelete_Task = nameof(CanDelete_Task);
    public const string CanView_Task = nameof(CanView_Task);

    #endregion Tasks

    #region Company

    public const string CanCreate_Company = nameof(CanCreate_Company);
    public const string CanUpdate_Company = nameof(CanUpdate_Company);
    public const string CanDelete_Company = nameof(CanDelete_Company);
    public const string CanView_Company = nameof(CanView_Company);

    #endregion Company

    #region Boards

    public const string CanCreate_BoardSet = nameof(CanCreate_BoardSet);
    public const string CanUpdate_BoardSet = nameof(CanUpdate_BoardSet);
    public const string CanDelete_BoardSet = nameof(CanDelete_BoardSet);
    public const string CanView_BoardSet = nameof(CanView_BoardSet);

    public const string CanCreate_BoardItem = nameof(CanCreate_BoardItem);
    public const string CanUpdate_BoardItem = nameof(CanUpdate_BoardItem);
    public const string CanDelete_BoardItem = nameof(CanDelete_BoardItem);
    public const string CanView_BoardItem = nameof(CanView_BoardItem);

    #endregion Boards

    #region ClaimsConfig

    public const string CanCreate_ClaimsConfig = nameof(CanCreate_ClaimsConfig);
    public const string CanUpdate_ClaimsConfig = nameof(CanUpdate_ClaimsConfig);
    public const string CanDelete_ClaimsConfig = nameof(CanDelete_ClaimsConfig);
    public const string CanView_ClaimsConfig = nameof(CanView_ClaimsConfig);

    #endregion ClaimsConfig

    #region ClaimsSubmission

    public const string CanCreate_ClaimsSubmission = nameof(CanCreate_ClaimsSubmission);
    public const string CanUpdate_ClaimsSubmission = nameof(CanUpdate_ClaimsSubmission);
    public const string CanDelete_ClaimsSubmission = nameof(CanDelete_ClaimsSubmission);
    public const string CanView_ClaimsSubmission = nameof(CanView_ClaimsSubmission);
    public const string CanReview_ClaimsSubmission = nameof(CanReview_ClaimsSubmission);

    #endregion ClaimsSubmission
}
