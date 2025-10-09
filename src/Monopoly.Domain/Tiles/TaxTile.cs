using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Tiles
{
    public sealed class TaxTile : Tile
    {
        public int Amount { get; }

        public TaxTile(int index, string name, int amount, Guid? tileId = null)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
            Amount = amount;
        }

        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
            // Thu thuế: trừ tiền người chơi
            ctx.Bus.Publish(new FundsChanged(player.Id, -Amount, $"Tax: {Name}", player.Cash));
        }
    }
}
