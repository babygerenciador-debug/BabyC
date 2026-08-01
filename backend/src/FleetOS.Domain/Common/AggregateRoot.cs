using MediatR;

namespace FleetOS.Domain.Common;

/// <summary>
/// Aggregate root with domain events support.
/// All aggregate roots MUST inherit from this class.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot() { }

    protected AggregateRoot(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId)
        : base(id, tenantId, organizationId, businessUnitId) { }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>Marker interface for domain events.</summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

/// <summary>Base record for domain events.</summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
