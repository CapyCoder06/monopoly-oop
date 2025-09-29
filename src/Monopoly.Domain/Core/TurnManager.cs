using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.State;

namespace Monopoly.Domain.Core
{
    public static class GameRules
    {
        public const int JailBailAmount = 50;
    }
}

public class TurnManager
{
    private readonly Board _board;
    private readonly IDice _dice;
    private readonly IDomainEventBus _bus;

    public TurnManager(Board board, IDice dice, IDomainEventBus bus)
    {
        _board = board;
        _dice = dice;
        _bus = bus;
    }

    // Hàm RollDiceAndAdvance: Quay xúc xắc và di chuyển người chơi trên bảng. 
    //roll -> di chuyển -> publish sự kiện PlayerMoved
    public (int sum, bool isDouble) RollDiceAndAdvance(Player player, GameContext ctx, JailReleaseAction action = JailReleaseAction.None)
    {
        var (d1, d2, sum, isDouble) = _dice.Roll();
        if (player.CurrentState is IPlayerState state)
        {
            if (state.OnRollDice(player, ctx, d1, d2, action))
            {
                return (sum, isDouble);
            }
        }
        var from = player.Position;
        player.Move(sum, _board.Size);
        _bus.Publish(new PlayerMoved(player.Id, from, player.Position));
        _board.Tiles[player.Position].OnLand(ctx, player, sum);
        return (sum, isDouble);
    }
}


