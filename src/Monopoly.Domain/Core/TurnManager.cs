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

        // Nếu đang ở JAIL: xử lý theo luật chuẩn
        if (player.CurrentState is InJailState jail)
        {
            // 1) Hành động chủ động: Pay bail / Use card → rời Jail & MOVE theo tổng
            if (action == JailReleaseAction.PayBail)
            {
                if (player.TryDebit(GameRules.JailBailAmount))
                {
                    _bus.Publish(new BailPaid(player.Id, GameRules.JailBailAmount));
                    player.CurrentState = new NormalState();
                    _bus.Publish(new LeftJail(player.Id, JailLeaveReason.BailPaid));
                    Move(sum);
                }
                return (sum, isDouble);
            }

            if (action == JailReleaseAction.UseCard && player.TryConsumeJailCard())
            {
                _bus.Publish(new UsedJailCard(player.Id));
                player.CurrentState = new NormalState();
                _bus.Publish(new LeftJail(player.Id, JailLeaveReason.UsedCard));
                Move(sum);
                return (sum, isDouble);
            }

            // 2) Không action: thử đổ đôi
            if (isDouble)
            {
                _bus.Publish(new RolledDoubleToLeave(player.Id, d1 + d2));
                player.CurrentState = new NormalState();
                _bus.Publish(new LeftJail(player.Id, JailLeaveReason.RolledDouble));
                Move(sum);
                return (sum, isDouble);
            }

            // 3) Không đôi
            if (jail.TurnsLeft > 1)
            {
                // Lượt 1–2: KHÔNG MOVE, chỉ giảm lượt
                jail.Decrement();
                return (sum, isDouble);
            }

            // 4) Lượt 3 không đôi: auto-bail rồi MOVE theo tổng
            if (player.TryDebit(GameRules.JailBailAmount))
            {
                _bus.Publish(new BailPaid(player.Id, GameRules.JailBailAmount));
                player.CurrentState = new NormalState();
                _bus.Publish(new LeftJail(player.Id, JailLeaveReason.AfterThreeTurns));
                Move(sum);
            }
            return (sum, isDouble);
        }

        // Không ở Jail: lăn & MOVE bình thường
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


