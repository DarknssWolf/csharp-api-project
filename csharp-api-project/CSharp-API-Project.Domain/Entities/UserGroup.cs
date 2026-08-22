using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;
public class UserGroup : BaseAuditableEntity
{
    // PK
    public Guid GroupId { get; set; }
    public int VendorId { get; set; }

    // Properties
    public string Name { get; set; }
    public string? Description { get; set; }
    public int OrderNumber { get; set; }

}
