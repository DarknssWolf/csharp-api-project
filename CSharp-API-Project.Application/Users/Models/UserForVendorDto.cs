using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Users.Models;

public class UserForVendorDto
{
	public string Id { get; set; }
	public int VendorId { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string? Phone { get; set; }
	public UserStatus Status { get; set; }
	public IList<string> Roles { get; set; } = [];
	public string? UserGroupId { get; set; }
	public string? UserSubGroupId { get; set; }
	public Language? Language { get; set; }
	public DateTimeOffset? CreationDate { get; set; }
}
