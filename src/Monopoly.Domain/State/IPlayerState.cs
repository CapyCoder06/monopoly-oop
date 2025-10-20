using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.State;

public enum JailReleaseAction { None, PayBail, UseCard }

public interface IPlayerState
{
    bool OnRollDice(Player p, GameContext ctx, int d1, int d2, JailReleaseAction action);
    void OnEndTurn(Player p, GameContext ctx);
}
public class NormalState : IPlayerState
{
    public bool OnRollDice(Player p, GameContext ctx, int dice1, int dice2, JailReleaseAction action) => true;
    public void OnEndTurn(Player p, GameContext ctx) { }
}
public class InJailState : IPlayerState
{
    public int TurnsLeft { get; private set; }
    public InJailState(int turns = 3)
    {
        TurnsLeft = turns;
    }
    public void Decrement()
    {
        if (TurnsLeft > 0)
            TurnsLeft--;
    }
    private void Leave(Player player, GameContext ctx, JailLeaveReason reason)
    {
        player.CurrentState = new NormalState();
        ctx.Bus.Publish(new LeftJail(player.Id, reason));
    }
    public bool OnRollDice(Player p, GameContext ctx, int d1, int d2, JailReleaseAction a)
    {
        if (a == JailReleaseAction.UseCard && p.TryConsumeJailCard())
        {
            ctx.Bus.Publish(new UsedJailCard(p.Id));
            Leave(p, ctx, JailLeaveReason.UsedCard);
            return true;
        }
        if (a == JailReleaseAction.PayBail && p.TryDebit(GameRules.JailBailAmount))
        {
            ctx.Bus.Publish(new BailPaid(p.Id, GameRules.JailBailAmount));
            Leave(p, ctx, JailLeaveReason.BailPaid);
            return true;
        }
        if (d1 == d2)
        {
            ctx.Bus.Publish(new RolledDoubleToLeave(p.Id, d1 + d2));
            Leave(p, ctx, JailLeaveReason.RolledDouble);
            return true;
        }
        if (TurnsLeft > 0) TurnsLeft--;
        if (TurnsLeft < 1)
        {
            if (p.TryDebit(GameRules.JailBailAmount))
            {
                ctx.Bus.Publish(new BailPaid(p.Id, GameRules.JailBailAmount));
                Leave(p, ctx, JailLeaveReason.AfterThreeTurns);
                return true;
            }
            return false;
        }
        return false;
    }
    public void OnEndTurn(Player p, GameContext ctx) { }
}
public class BankruptState : IPlayerState
{
    public bool OnRollDice(Player p, GameContext ctx, int d1, int d2, JailReleaseAction action) => false;
    public void OnEndTurn(Player p, GameContext ctx) { }
}
