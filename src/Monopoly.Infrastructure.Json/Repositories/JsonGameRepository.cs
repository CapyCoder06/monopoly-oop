using Monopoly.Application.Ports;

namespace Monopoly.Infrastructure.Json.Repositories;

// Mốc 0: tạm In-Memory để pass build; mốc 5 sẽ đọc/ghi JSON
public class JsonGameRepository : IGameRepository
{
    private readonly Dictionary<string, GameSnapshot> _db = new();
    public GameSnapshot? Load(string slot) => _db.TryGetValue(slot, out var s) ? s : null;
    public void Save(GameSnapshot snapshot) => _db[snapshot.Slot] = snapshot;
}
