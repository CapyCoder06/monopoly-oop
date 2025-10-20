using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Tiles;

namespace Monopoly.Domain.Strategy;
public class PropertyRentStrategy : IRentStrategy
{
    public int BaseRent { get; }
    public PropertyRentStrategy(int baseRent)
    {
        BaseRent = baseRent;
    }
    public int CalculateRent(GameContext ctx, Tile tile, int lastDiceSum)
    {
        if (tile is not PropertyTile p)
        {
            return 0;
        }
        if (!p.OwnerId.HasValue)
        {
            return 0;
        }
        bool hasFullSet = ctx.OwnerHasFullColorSet(p.OwnerId.Value, p.Color);
        if (hasFullSet)
        {
            return p.BaseRent * 2;
        }
        return p.BaseRent;
    }
}
