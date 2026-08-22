namespace CSharpAPIProject.Domain.Enums
{
	public enum WhoCanCreateVoteItemFlag
	{
		/// <summary>
		/// Users & Admins can upload things to vote on, and vote on things
		/// </summary>
		User = 0,
		/// <summary>
		/// Only Admins can upload things to vote on, but users can vote
		/// </summary>
		Admin = 1,
	}
}
