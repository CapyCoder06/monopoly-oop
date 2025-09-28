using Monopoly.Domain.Abstractions;
namespace Monopoly.Domain.Core;

public class Board
{
    public List<Tile> Tiles { get; }
    public int Size => Tiles.Count;
    public Board(List<Tile> tiles)
    {
        Tiles = tiles;
    }

}
