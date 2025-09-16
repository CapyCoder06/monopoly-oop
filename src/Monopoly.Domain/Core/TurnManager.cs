using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Core;

public class TurnManager
{
    private readonly Board _board;
    private readonly IDice _dice;
    private readonly IDomainEventBus _bus;

    public TurnManager(Board board, IDice dice, IDomainEventBus bus)
    {
        _board = board; _dice = dice; _bus = bus;
    }

    public (int sum, bool isDouble) RollDiceAndAdvance(Player player)
    {
        var (d1, d2, sum, isDouble) = _dice.Roll();
        var from = player.Position;
        player.Move(sum, _board.Size);
        _bus.Publish(new PlayerMoved(player.Id, from, player.Position));
        return (sum, isDouble);
    }
}
