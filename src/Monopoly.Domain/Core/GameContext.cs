using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Tiles;
using Monopoly.Domain.Events; 

namespace Monopoly.Domain.Core;

public sealed class GameContext
{
    public Board Board { get; }
    public IDomainEventBus Bus { get; }
    public IWallet Wallet { get; }
    public GameContext(Board board, IDomainEventBus bus, IWallet wallet)
    {
        Board = board;
        Bus = bus;
        Wallet = wallet;
    }

    public bool OwnerHasFullColorSet(Guid OwnerId, PropertyColor color)
    {
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is PropertyTile propertyTile && propertyTile.Color == color)
            {
                if (propertyTile.OwnerId != OwnerId)
                    return false;
            }
        }
        return true;
    }

    public int CountOwnerRailroads(Guid OwnerId)
    {
        int count = 0;
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is RailroadTile railroadTile && railroadTile.OwnerId == OwnerId)
            {
                count++;
            }
        }
        return count;
    }
    public int CountOwnerUtilities(Guid OwnerId)
    {
        int count = 0;
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is UtilityTile utilityTile && utilityTile.OwnerId == OwnerId)
            {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Tìm ô gần nhất (theo chiều kim đồng hồ) thỏa predicate, bắt đầu sau fromIndex.
    /// Trả về index ô tìm được. Ném InvalidOperationException nếu không tìm thấy.
    /// </summary>
    public int FindNearest(Predicate<Tile> predicate, int fromIndex)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (Board is null || Board.Tiles is null || Board.Tiles.Count == 0)
            throw new InvalidOperationException("Board is not initialized.");

        int n = Board.Tiles.Count;
        for (int step = 1; step <= n; step++)
        {
            int idx = (fromIndex + step) % n;
            var tile = Board.Tiles[idx];
            if (predicate(tile))
            {
                return idx;
            }
        }
        // Không tìm thấy ô nào thỏa điều kiện
        throw new InvalidOperationException("No matching tile found for the given predicate.");
    }
    /// <summary>
    /// Di chuyển player tới targetIndex theo “Move by Card”.
    /// - Tính passedGo và bonus (nếu cấu hình withGoBonusPolicy = true).
    /// - Cập nhật Position.
    /// - Publish MovedByCard và (nếu có) FundsChanged(+bonus) thông qua Wallet.
    /// - Trả về (from, to, passedGo, bonus).
    /// </summary>
    public (int from, int to, bool passedGo, int bonus)
        ApplyMoveCard(Player player, int targetIndex, bool withGoBonusPolicy, int goBonusAmount = 200, string reason = "Move By Card")
    {
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (Board is null || Board.Tiles is null || Board.Tiles.Count == 0)
            throw new InvalidOperationException("Board is not initialized.");

        int n = Board.Tiles.Count;
        if (targetIndex < 0 || targetIndex >= n)
            throw new ArgumentOutOfRangeException(nameof(targetIndex), "Target index out of board range.");

        int from = player.Position;
        // passedGo khi đi theo chiều kim đồng hồ và target “vòng qua 0”
        bool passedGo = (targetIndex <= from); //ví dụ from=38 -> target=5
        int bonus = 0;

        // Thưởng GO nếu policy bật và có vượt qua GO
        if (withGoBonusPolicy && passedGo && goBonusAmount > 0)
        {
            Wallet.Credit(player, goBonusAmount, "Passed GO Bonus");
            bonus = goBonusAmount;
        }


        // Cập nhật vị trí
        player.Position = targetIndex;

        // Ghi nhận movement do thẻ (nếu bạn có event MovedByCard “chuẩn hóa” thì truyền thêm fields cần thiết)
        Bus.Publish(new MovedByCard(player.Id, from, targetIndex, reason, passedGo, bonus));

        return (from, targetIndex, passedGo, bonus);
    }

}
