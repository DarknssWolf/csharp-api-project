using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteSets.Models;

public class VoteSetItemDto
{
	public int? VoteItemId { get; set; } //Optional as it is set on insert, but could be passed in for updates
	public int VoteSetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public VoteItemStatus? Status { get; set; } = VoteItemStatus.Pending;
	public string? VoteItemStatusReason { get; set; }
	public string? VoteItemData { get; set; } // String containing image/video data
    public string CreatorName { get; set; } = String.Empty;
    public bool IsAuthor { get; set; } = false;
	public int VoteSubmissionsCount { get; set; }
	public bool UserSubmission { get; set; } = false;
    public Guid VisibilityConfigurationId { get; set; }
}
