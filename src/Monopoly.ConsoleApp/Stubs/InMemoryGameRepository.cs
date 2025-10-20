using System.Collections.Concurrent;
using Monopoly.Application.Ports; 
using Monopoly.Domain.Core;

namespace Monopoly.ConsoleApp
{
    public sealed class InMemoryGameRepository : IGameRepository
    {
        private readonly ConcurrentDictionary<string, GameSnapshot> _store = new ConcurrentDictionary<string, GameSnapshot>();

        public void Save(GameSnapshot snapshot)
        {
            _store[snapshot.Slot] = snapshot;
        }

        public GameSnapshot? Load(string slot)
        {
            return _store.TryGetValue(slot, out var snap) ? snap : null;
        }
    }
}