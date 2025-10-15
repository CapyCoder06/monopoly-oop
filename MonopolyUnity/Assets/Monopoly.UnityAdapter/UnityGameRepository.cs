using Monopoly.Application.Ports;
using System.Collections.Generic;

namespace Monopoly.UnityAdapter
{
    public class UnityGameRepository : IGameRepository
    {
        private readonly Dictionary<string, GameSnapshot> _storage = new();

        public GameSnapshot? Load(string slot)
        {
            _storage.TryGetValue(slot, out var snapshot);
            return snapshot;
        }

        public void Save(GameSnapshot snapshot)
        {
            _storage[snapshot.Slot] = snapshot;
        }
    }
}
