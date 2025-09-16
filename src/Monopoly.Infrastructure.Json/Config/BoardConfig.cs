namespace Monopoly.Infrastructure.Json.Config;

// Dùng để deserialize file /Config/BoardConfig.json ở mốc 5
public class BoardConfig
{
    public int Size { get; set; } = 40;
    public List<TileConfig> Tiles { get; set; } = new();
}

public class TileConfig
{
    public int Index { get; set; }
    public string Kind { get; set; } = "Property";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Props { get; set; } = new();
}
