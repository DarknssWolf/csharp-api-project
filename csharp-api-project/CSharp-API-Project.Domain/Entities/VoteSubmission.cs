using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;

public class VoteSubmission : BaseAuditableEntity
{
	//Pk
	public int VoteSubmissionId { get; set; } 
	//FK
	public int VoteItemId { get; set; }

	//Navigation Properties
	public VoteItem VoteItem { get; set; }
};
