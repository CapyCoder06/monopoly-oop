using Monopoly.Domain.Core;
using Monopoly.Domain.Tiles;
namespace Monopoly.Domain.Abstractions;
public abstract class Tile
{
    public int Index { get; }
    public string Name { get; }
    public Guid TileId { get; } = Guid.NewGuid();

    protected Tile(int index, string name, Guid tileId)
    {
        Index = index;
        Name = name;
        TileId = tileId;
    }
    public abstract void OnLand(GameContext ctx, Player player, int lastDiceSum);
}

