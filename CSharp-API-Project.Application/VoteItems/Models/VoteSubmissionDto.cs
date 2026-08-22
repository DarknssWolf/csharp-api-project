using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteItems.Models;

public class VoteSubmissionDto
{
	public int VoteSubmissionId { get; set; }
	public int VotingItemId { get; set; }
	public int CustomerId { get; set; }
	public string CustomerName { get; set; }
	public string? CustomerEmail { get; set; }
}
