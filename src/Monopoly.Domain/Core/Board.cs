namespace Monopoly.Domain.Core;

public class Board
{
    public int Size { get; }
    public Board(int size = 40) => Size = size;
}
