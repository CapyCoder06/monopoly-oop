using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
namespace Monopoly.Domain.Strategy;

public interface IRentStrategy
{
    int CalculateRent(GameContext ctx, Tile tile, int lastDiceSum);
}
