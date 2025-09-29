using System.Reflection.Metadata.Ecma335;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Strategy;


namespace Monopoly.Domain.Tiles;

public class JailTile : Tile
{
    public JailTile(int index, string name = "Jail", Guid? tileId = null)
        : base(index, name, tileId ?? Guid.NewGuid())
    {
    }
    public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
    {
        //Just Visiting
    }
}