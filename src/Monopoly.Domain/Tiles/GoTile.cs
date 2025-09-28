using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;

namespace Monopoly.Domain.Tiles;

public class GoTile : Tile
{
    public GoTile(int index = 0, string name = "GO", Guid? tileId = null)
        : base(index, name, tileId ?? Guid.NewGuid())
    {    
    }

    public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
    {
        player.Receive(200);
    }
}
