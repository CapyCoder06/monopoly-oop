using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Tiles;

namespace Monopoly.Domain.Strategy;

public class UtilityRentStrategy : IRentStrategy
{
    public int CalculateRent(GameContext ctx, Tile tile, int lastDiceSum)
    {
        if (tile is not UtilityTile u)
        {
            return 0;
        }
        if (!u.OwnerId.HasValue)
        {
            return 0; //chưa có ai mua
        }
        //kiểm tra chủ sở hữu có sở hữu cả 2 tiện ích không
        int utilityCount = ctx.CountOwnerUtilities(u.OwnerId.Value);
        if (utilityCount == 2)
        {
            return lastDiceSum * 10;
        }
        return lastDiceSum * 4;
    }
}