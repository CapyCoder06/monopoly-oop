namespace Monopoly.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public record PlayerMoved(Guid PlayerId, int From, int To) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public interface IDomainEventBus
{
    void Publish(IDomainEvent @event);
    IReadOnlyList<IDomainEvent> DequeueAll();
}

public class InMemoryDomainEventBus : IDomainEventBus
{
    private readonly List<IDomainEvent> _events = new();
    public void Publish(IDomainEvent @event) => _events.Add(@event);
    public IReadOnlyList<IDomainEvent> DequeueAll()
    {
        var copy = _events.ToList();
        _events.Clear();
        return copy;
    }
}
