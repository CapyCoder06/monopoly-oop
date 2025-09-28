using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Tiles;

namespace Monopoly.Domain.Strategy;

public class RailRoadRentStrategy : IRentStrategy
{
    public int CalculateRent (GameContext ctx, Tile tile, int lastDiceSum)
    {
        if (tile is not RailroadTile r)
        {
            return 0;
        }
        if (!r.OwnerId.HasValue)
        {
            return 0; //chưa có ai mua
        }
        int ownedRailroads = ctx.CountOwnerRailroads(r.OwnerId.Value);
        return ownedRailroads switch
        {
            1 => 25,
            2 => 50,
            3 => 100,
            4 => 200,
            _ => 0
        };
    }

}