namespace CSharpAPIProject.Application.UserGroups.Models;
public class UserSubGroupDto
{
    public Guid SubGroupId { get; set; }
    public Guid? GroupId { get; set; } 
    public string Name { get; set; }
    public string? Description { get; set; }
    public int OrderNumber { get; set; }
}
