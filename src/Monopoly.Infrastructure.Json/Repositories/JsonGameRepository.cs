using System.Text.Json;
using System.Text.Json.Serialization;
using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Factory;

namespace Monopoly.Infrastructure.Json.Repositories;

public class JsonGameRepository : IGameRepository
{
    private readonly string _saveDir;
    private readonly JsonSerializerOptions _options;

    public JsonGameRepository(string? saveDir = null)
    {
        _saveDir = saveDir ?? Path.Combine(AppContext.BaseDirectory, "Saves");
        Directory.CreateDirectory(_saveDir);

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter() 
            }
        };
    }

    private string GetPath(string slot) => Path.Combine(_saveDir, $"{slot}.json");

    public void Save(GameSnapshot snapshot)
    {
        var path = GetPath(snapshot.Slot);

        var json = JsonSerializer.Serialize(snapshot, _options);
        File.WriteAllText(path, json);
    }

    public GameSnapshot? Load(string slot)
    {
        var path = GetPath(slot);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        var snapshot = JsonSerializer.Deserialize<GameSnapshot>(json, _options);

        if (snapshot is null)
            return null;

        var newBoard = new Board(TileFactory.CreateFromConfig());
        var rebuilt = snapshot with { board = newBoard };

        return rebuilt;
    }
}
