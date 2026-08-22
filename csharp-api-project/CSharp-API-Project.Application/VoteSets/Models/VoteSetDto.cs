using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteSets.Models;

public class VoteSetDto
{
	public int VoteSetId { get; set; }
	public string Title { get; set; }
	public string Code { get; set; }
	public DateTime StartOn { get; set; }
	public DateTime EndOn { get; set; }
	public bool CanEarnPoints { get; set; }
	public bool AllowMultipleTries { get; set; }
	public bool AllowMultipleItems { get; set; }
	public string? EmailNotification { get; set; }
	public string? VoteSetData { get; set; }
    public bool? userHasUploaded { get; set; } = false;//indicator that the logged in user has uploaded to this voteset
	public WhoCanCreateVoteItemFlag WhoCanCreateVoteItemFlag { get; set; }
	public IEnumerable<VoteSetItemDto>? VoteItems { get; set; }
    public Guid VisibilityConfigurationId { get; set; }
}
