using Monopoly.Domain.Abstractions;

namespace Monopoly.Domain.Tiles;

public class GoTile : Tile
{
    public GoTile(int index = 0) : base(index, "GO") { }
}
