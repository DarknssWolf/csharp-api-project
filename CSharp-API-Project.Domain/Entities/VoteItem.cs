using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities;

public class VoteItem : BaseAuditableEntity
{
	//Pk
	public int VoteItemId { get; set; }
	//FK
    public int VoteSetId { get; set; } 
    public string Title { get; set; } // Title of the vote item
    public string Description { get; set; } 
	public VoteItemStatus Status { get; set; } = VoteItemStatus.Pending;
	public string? VoteItemStatusReason { get; set; }
	public string? VoteItemData { get; set; } // String containing image/video data
	public ICollection<VoteSubmission>? VoteSubmissions { get; set; }

	//Navigation Properties
	public VoteSet VoteSet { get; set; }
};
