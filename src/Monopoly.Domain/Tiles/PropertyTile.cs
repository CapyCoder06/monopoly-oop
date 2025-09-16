using Monopoly.Domain.Abstractions;

namespace Monopoly.Domain.Tiles;

public class PropertyTile : Tile
{
    public int BaseRent { get; }
    public PropertyTile(int index, string name, int baseRent = 0) : base(index, name)
    {
        BaseRent = baseRent;
    }
}
