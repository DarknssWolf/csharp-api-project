using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.ValueObjects;
public class EntityStatus : ValueObject
{
    public RecordStatus? Status { get; set; }
    public string? StatusUpdatedBy { get; set; }
    public DateTimeOffset StatusUpdatedOn { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Status;
        yield return StatusUpdatedBy;
        yield return StatusUpdatedOn;
    }

    public EntityStatus(RecordStatus status, string? statusUpdatedBy, DateTimeOffset statusUpdatedOn)
    {
        Status = status;
        StatusUpdatedBy = statusUpdatedBy;
        StatusUpdatedOn = statusUpdatedOn;
    }

    public EntityStatus()
    { } // Constructor exists for new empty objects. See baseAuditableEntity.cs
}
