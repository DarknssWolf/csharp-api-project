namespace CSharpAPIProject.Application.UserGroups.Models;
public class UserGroupDto
{
    public Guid GroupId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int OrderNumber { get; set; }

    public ICollection<UserSubGroupDto> UserSubGroups { get; set; }
}
