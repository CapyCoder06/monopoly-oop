using Monopoly.Domain.Events;
using System.Collections.Generic;

namespace Monopoly.UnityAdapter
{
    public class DomainEventBus : IDomainEventBus
    {
        private readonly Queue<IDomainEvent> _queue = new();

        public void Publish(IDomainEvent e)
        {
            _queue.Enqueue(e);
        }

        public IReadOnlyList<IDomainEvent> DequeueAll()
        {
            var list = _queue.ToArray();
            _queue.Clear();
            return list;
        }
    }
}
