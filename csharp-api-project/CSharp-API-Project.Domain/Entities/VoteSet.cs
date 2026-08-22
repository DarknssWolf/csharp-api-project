using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities;

public class VoteSet : BaseAuditableEntity
{
	// PK
	public int VoteSetId { get; set; }
	public int VendorId { get; set; }

	// Properties
	public string Title { get; set; }
	public string Code { get; set; }
	public DateTime StartOn { get; set; }
	public DateTime EndOn { get; set; }
	public bool CanEarnPoints { get; set; } = false;
	public bool AllowMultipleTries { get; set; } = false;
	public bool AllowMultipleItems { get; set; } = false;
	public string? EmailNotification { get; set; } //Comman delimited Enum of VoteEmailNotificationRole, validate on upsert
	public string? VoteSetData { get; set; } // String containing image/video data
	public WhoCanCreateVoteItemFlag WhoCanCreateVoteItemFlag { get; set; } //For if User or Admin (Could be expanded in future

	// Navigation Properties
	public ICollection<VoteItem> VoteItems { get; set; }
	public Vendor Vendor { get; set; }
};
