using System;
using System.Collections.Generic;
using System.Linq;
using Monopoly.Domain.Core;

namespace Monopoly.Domain.Events
{

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
    public record LandUnownedProperty(Guid TileId, int Price) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record RentDue(Guid PlayerId, Guid OwnerId, int Amount, Guid TileId) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    //Jail
    public record WentToJail(Guid PlayerId, int JailIndex) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public enum JailLeaveReason
    {
        RolledDouble,
        UsedCard,
        BailPaid,
        AfterThreeTurns
    }
    public record LeftJail(Guid PlayerId, JailLeaveReason Reason) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record BailPaid(Guid PlayerId, int Amount) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record UsedJailCard(Guid PlayerId) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record RolledDoubleToLeave(Guid PlayerId, int Sumdice) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public record FundsChanged(Guid PlayerId, int Amount, string Reason, int NewCash) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record CardDrawn(Guid TileId, Guid PlayerId, string CardTitle) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record CardResolved(string Title, Guid PlayerId, int AfterCash, int AfterPosition, string Effect) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record GotOutOfJailCardGranted(Guid PlayerId, int CountInHand) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
