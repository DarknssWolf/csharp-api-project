using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Domain.Entities;
public class UserSubGroup : BaseAuditableEntity
{
    // PK
    public Guid SubGroupId { get; set; }
    public int VendorId { get; set; }

    //FK
    public Guid? GroupId { get; set; } // eg, Country -> resident, service, company

    // Properties
    public string Name { get; set; }
    public string? Description { get; set; }
    public int OrderNumber { get; set; }

    //relationships
    public UserGroup UserGroup { get; set; }
}
