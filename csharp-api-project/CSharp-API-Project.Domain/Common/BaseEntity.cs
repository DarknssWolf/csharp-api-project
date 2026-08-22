using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpAPIProject.Domain.Common;

public abstract class BaseEntity
{
    private readonly List<BaseEvent> _domainEvents = [];
    private readonly List<BaseEvent> _postDomainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();
    [NotMapped]
    public IReadOnlyCollection<BaseEvent> PostDomainEvents => _postDomainEvents.AsReadOnly();

    public void AddPreDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void AddPostDomainEvent(BaseEvent domainEvent)
    {
        _postDomainEvents.Add(domainEvent);
    }

    public void RemovePreDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void RemovePostDomainEvent(BaseEvent domainEvent)
    {
        _postDomainEvents.Remove(domainEvent);
    }

    public void ClearPreDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void ClearPostDomainEvents()
    {
        _postDomainEvents.Clear();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
        _postDomainEvents.Clear();
    }
}
