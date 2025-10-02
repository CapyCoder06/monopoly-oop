// tests/Monopoly.Domain.Tests/Moc3.cs
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

using Monopoly.Domain.Core;     // Board, GameContext, Player, TurnManager, IDice
using Monopoly.Domain.Events;   // WentToJail, LeftJail, BailPaid, UsedJailCard, RolledDoubleToLeave, PlayerMoved
using Monopoly.Domain.Tiles;    // JailTile, GoToJailTile, Tile
using Monopoly.Domain.State;    // InJailState, NormalState
using Monopoly.Domain.Abstractions;

// No-op tile để lấp chỗ, không side-effect khi OnLand
internal sealed class NoopTile : Tile
{
    public NoopTile(int index, string name = "Noop", Guid? tileId = null)
        : base(index, name, tileId ?? Guid.NewGuid()) { }
    public override void OnLand(GameContext ctx, Player player, int lastDiceSum) { /* no-op */ }
}

// Dice cố định để kiểm soát kết quả roll trong test
internal sealed class FixedDice : IDice
{
    private readonly Queue<(int d1, int d2, int sum, bool isDouble)> _q = new();
    public FixedDice(params (int d1, int d2)[] seq)
    {
        foreach (var (d1, d2) in seq)
            _q.Enqueue((d1, d2, d1 + d2, d1 == d2));
    }
    public (int d1, int d2, int sum, bool isDouble) Roll()
        => _q.Count > 0 ? _q.Dequeue() : (1, 1, 2, true);
}

public class Moc3
{
    private static Board MakeBoard(params Tile[] tiles)
        => new Board(new List<Tile>(tiles));

    private static GameContext MakeCtx(Board board, out InMemoryDomainEventBus bus)
    {
        bus = new InMemoryDomainEventBus();
        var wallet = new InMemoryWallet(bus);
        return new GameContext(board, bus, wallet);
    }

    // 1) GoToJail: land → InJail(3), token đến JailIndex, WentToJail
    [Fact]
    public void LandOn_GoToJail_Should_Move_To_Jail_And_Set_InJailState_With_3_Turns()
    {
        var jail = new JailTile(index: 10, name: "Jail");
        var gtj  = new GoToJailTile(index: 0, jailIndex: 10, name: "GoToJail");
        var board = MakeBoard(gtj, new NoopTile(1), jail);

        var ctx = MakeCtx(board, out var bus);
        var p = new Player("P1") { Position = 0 };

        gtj.OnLand(ctx, p, lastDiceSum: 0);
        var events = bus.DequeueAll().ToList();

        Assert.IsType<InJailState>(p.CurrentState);
        Assert.Equal(3, ((InJailState)p.CurrentState).TurnsLeft);
        Assert.Equal(10, p.Position);
        Assert.Contains(events, e => e is WentToJail w && w.PlayerId == p.Id && w.JailIndex == 10);
    }

    // 2) Pay bail vào đầu lượt: rời Jail và DI CHUYỂN theo tổng
    [Fact]
    public void PayBail_Should_Leave_Jail_And_Move_By_Sum()
    {
        var jail = new JailTile(10);
        var gtj  = new GoToJailTile(0, 10);
        var board = MakeBoard(gtj, new NoopTile(1), jail, new NoopTile(11), new NoopTile(12), new NoopTile(13), new NoopTile(14), new NoopTile(15));
        var ctx = MakeCtx(board, out var bus);
        var p = new Player("P1", startingCash: 1500) { Position = 0 };

        // vào tù
        gtj.OnLand(ctx, p, 0);
        bus.DequeueAll();

        // trả bail và move theo sum 5
        var dice = new FixedDice((2, 3));
        var tm = new TurnManager(board, dice, bus);

        var (sum, isDouble) = tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.PayBail);
        var events = bus.DequeueAll().ToList();

        Assert.Equal(5, sum);
        Assert.False(isDouble);
        Assert.IsType<NormalState>(p.CurrentState);
        Assert.Equal(1500 - GameRules.JailBailAmount, p.Cash);
        Assert.Contains(events, e => e is BailPaid b && b.PlayerId == p.Id && b.Amount == GameRules.JailBailAmount);
        Assert.Contains(events, e => e is LeftJail l && l.PlayerId == p.Id);

        // đã move theo sum 5
        Assert.Contains(events, e => e is PlayerMoved mv && mv.PlayerId == p.Id && mv.From == 10 && mv.To == (10 + 5) % board.Tiles.Count);
    }

    // 3) Use card: rời Jail và DI CHUYỂN theo tổng
    [Fact]
    public void UseCard_Should_Leave_Jail_And_Move_By_Sum()
    {
        var board = MakeBoard(new GoToJailTile(0, 10), new NoopTile(1), new JailTile(10), new NoopTile(11), new NoopTile(12), new NoopTile(13), new NoopTile(14), new NoopTile(15));
        var ctx = MakeCtx(board, out var bus);
        var p = new Player("P1") { Position = 0 };

        ((GoToJailTile)board.Tiles[0]).OnLand(ctx, p, 0);
        bus.DequeueAll();

        p.GrantJailCard();
        Assert.True(p.JailCard > 0);

        // dùng thẻ, roll 4+1=5, rời Jail và move 5
        var tm = new TurnManager(board, new FixedDice((4, 1)), bus);
        tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.UseCard);

        var events = bus.DequeueAll().ToList();
        Assert.IsType<NormalState>(p.CurrentState);
        Assert.Contains(events, e => e is UsedJailCard u && u.PlayerId == p.Id);
        Assert.Contains(events, e => e is LeftJail l && l.PlayerId == p.Id);
        Assert.Contains(events, e => e is PlayerMoved mv && mv.From == 10 && mv.To == (10 + 5) % board.Tiles.Count);
    }

    // 4) Roll đôi: rời Jail và DI CHUYỂN theo tổng
    [Fact]
    public void RollDoubles_Should_Leave_Jail_And_Move_By_Sum()
    {
        var jail = new JailTile(10);
        var gtj  = new GoToJailTile(0, 10);
        var board = MakeBoard(gtj, jail, new NoopTile(11), new NoopTile(12), new NoopTile(13), new NoopTile(14), new NoopTile(15), new NoopTile(16));
        var ctx = MakeCtx(board, out var bus);
        var p = new Player("P1") { Position = 0 };

        gtj.OnLand(ctx, p, 0);
        bus.DequeueAll();

        var tm = new TurnManager(board, new FixedDice((3, 3)), bus);
        tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.None);

        var events = bus.DequeueAll().ToList();
        Assert.IsType<NormalState>(p.CurrentState);
        Assert.Contains(events, e => e is RolledDoubleToLeave);
        Assert.Contains(events, e => e is LeftJail l && l.PlayerId == p.Id);
        Assert.Contains(events, e => e is PlayerMoved mv && mv.From == 10 && mv.To == (10 + 6) % board.Tiles.Count);
    }

    // 5) Không đôi 2 lượt đầu → không move; Lượt 3 không đôi → auto-bail và DI CHUYỂN theo tổng lượt 3
    [Fact]
    public void After3Turns_NoDouble_Should_Auto_Bail_And_Move_By_Third_Sum()
    {
        var jail = new JailTile(10);
        var gtj  = new GoToJailTile(0, 10);
        var board = MakeBoard(gtj, new NoopTile(1), jail,
                              new NoopTile(11), new NoopTile(12), new NoopTile(13), new NoopTile(14), new NoopTile(15), new NoopTile(16), new NoopTile(17), new NoopTile(18), new NoopTile(19));
        var ctx = MakeCtx(board, out var bus);
        var p = new Player("P1", startingCash: 2000) { Position = 0 };

        gtj.OnLand(ctx, p, 0);
        bus.DequeueAll();

        // 2 lượt đầu non-double (không move), lượt 3 non-double → auto-bail và move
        var tm = new TurnManager(board, new FixedDice((1,2), (2,3), (4,5)), bus);

        // turn 1
        tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.None);
        var ev1 = bus.DequeueAll().ToList();
        Assert.DoesNotContain(ev1, e => e is PlayerMoved);                  // KHÔNG move
        Assert.IsType<InJailState>(p.CurrentState);
        Assert.Equal(2, ((InJailState)p.CurrentState).TurnsLeft);           // 3 -> 2

        // turn 2
        tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.None);
        var ev2 = bus.DequeueAll().ToList();
        Assert.DoesNotContain(ev2, e => e is PlayerMoved);                  // KHÔNG move
        Assert.IsType<InJailState>(p.CurrentState);
        Assert.Equal(1, ((InJailState)p.CurrentState).TurnsLeft);           // 2 -> 1

        // turn 3 (auto-bail 50$) và MOVE theo sum=9
        tm.RollDiceAndAdvance(p, ctx, JailReleaseAction.None);
        var ev3 = bus.DequeueAll().ToList();

        Assert.IsType<NormalState>(p.CurrentState);
        Assert.Equal(2000 - GameRules.JailBailAmount, p.Cash);
        Assert.Contains(ev3, e => e is BailPaid b && b.PlayerId == p.Id && b.Amount == GameRules.JailBailAmount);
        Assert.Contains(ev3, e => e is LeftJail l && l.PlayerId == p.Id);
        Assert.Contains(ev3, e => e is PlayerMoved mv && mv.From == 10 && mv.To == (10 + 9) % board.Tiles.Count);
    }
}
