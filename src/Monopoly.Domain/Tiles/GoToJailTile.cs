using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Strategy;
using Monopoly.Domain.State;

namespace Monopoly.Domain.Tiles;

public class GoToJailTile : Tile
{
    public int JailIndex { get; }
    public GoToJailTile(int index, int jailIndex, string name = "Go to Jail", Guid? tileId = null) 
        : base(index, name, tileId ?? Guid.NewGuid()) {
        JailIndex = jailIndex;
    }
    public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
    {
        player.CurrentState = new InJailState(3);
        player.Position = JailIndex;
        ctx.Bus.Publish(new WentToJail(player.Id, JailIndex));
    }
}