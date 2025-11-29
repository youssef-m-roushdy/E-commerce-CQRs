namespace E_commerce.Domain.Events;

/// <summary>
/// Base class for all domain events
/// Domain events represent something that happened in the domain that domain experts care about
/// </summary>
public abstract class BaseDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }

    protected BaseDomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
