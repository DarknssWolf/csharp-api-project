using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Domain.Common;
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModifiedOn { get; set; }

    public string? LastModifiedBy { get; set; }
    public EntityStatus StatusFlag { get; set; } = new EntityStatus();

    public Guid? VisibilityConfigurationId { get; set; }
}

