using System.Collections.Concurrent;
using Monopoly.Application.Ports; // đúng namespace của IGameRepository
using Monopoly.Domain.Core;

namespace Monopoly.ConsoleApp;

// In-memory repository cho Mốc 0
public sealed class InMemoryGameRepository : IGameRepository
{
    // Key = Slot
    private readonly ConcurrentDictionary<string, GameSnapshot> _store = new();

    public void Save(GameSnapshot snapshot)
    {
        _store[snapshot.Slot] = snapshot;
    }

    public GameSnapshot? Load(string slot)
    {
        return _store.TryGetValue(slot, out var snap) ? snap : null;
    }
}
