namespace Monopoly.Domain.Core;

public interface IDice
{
    (int d1, int d2, int sum, bool isDouble) Roll();
}

public class Dice : IDice
{
    private readonly Random _rng = new();
    public (int d1, int d2, int sum, bool isDouble) Roll()
    {
        var d1 = _rng.Next(1, 7);
        var d2 = _rng.Next(1, 7);
        return (d1, d2, d1 + d2, d1 == d2);
    }
}
