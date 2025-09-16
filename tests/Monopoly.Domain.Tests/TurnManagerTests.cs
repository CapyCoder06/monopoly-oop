// tests/Monopoly.Domain.Tests/TurnManagerTests.cs
using System.Linq;                         // <-- thêm dòng này
using FluentAssertions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Xunit;

file class FakeDice : IDice
{
    private readonly int _sum; private readonly bool _dbl;
    public FakeDice(int sum, bool dbl = false) { _sum = sum; _dbl = dbl; }
    public (int d1, int d2, int sum, bool isDouble) Roll() => (0, 0, _sum, _dbl);
}

public class TurnManagerTests
{
    [Fact]
    public void Roll_Moves_Player_And_Raises_PlayerMoved()
    {
        var board = new Board(40);
        var dice  = new FakeDice(5);
        var bus   = new InMemoryDomainEventBus();
        var tm    = new TurnManager(board, dice, bus);
        var p     = new Player("A");

        var (sum, isDouble) = tm.RollDiceAndAdvance(p);

        sum.Should().Be(5);
        isDouble.Should().BeFalse();
        p.Position.Should().Be(5);

        var ev = bus.DequeueAll().OfType<PlayerMoved>().Single();
        ev.From.Should().Be(0);
        ev.To.Should().Be(5);
    }
}
