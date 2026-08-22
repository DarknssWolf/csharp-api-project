namespace CSharpAPIProject.Domain.Enums
{
	public enum VoteItemStatus
	{
		/// <summary>
		/// Approved submission
		/// </summary>
		Pending = 0,
		/// <summary>
		/// Declined submission
		/// </summary>
		Decline = 1,
		/// <summary>
		/// Waiting for approval
		/// </summary>
		Approve = 2,
	}
}
