namespace CSharpAPIProject.Application.Common.Models;

public class UserAccount
{
	public string Id { get; set; }
	public string Email { get; set; }
	public string UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public int TenantId { get; set; }
	public DateTimeOffset? CreationDate { get; set; }
}
