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

        bool canMove = player.CurrentState.OnRollDice(player, ctx, d1, d2, action);
        if (canMove)
            Move(sum);
        return (sum, isDouble);

        // Helper local: di chuyển + publish + OnLand
        void Move(int steps)
        {
            int size = _board.Size;
            var from = player.Position;
            int to = (from + steps) % size;
            bool passedGo = steps > 0 && to < from;
            if (passedGo)
            {
                player.Receive(200);
                _bus.Publish(new FundsChanged(player.Id, +200, "PassedGO", player.Cash));
            }
            player.Move(steps, _board.Size);
            _bus.Publish(new PlayerMoved(player.Id, from, player.Position));
            _board.Tiles[player.Position].OnLand(ctx, player, sum);
        }
    }
}


