using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Strategy;

namespace Monopoly.Domain.Tiles
{
    public class UtilityTile : Tile
    {
        public int BaseRent { get; } 
        public Guid? OwnerId { get; set; }
        public int Price { get; }
        public IRentStrategy rentStrategy;
        public UtilityTile(int index, string name, int price, Guid? tileId = null, int baseRent = 0)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
            BaseRent = baseRent;
            Price = price;
            rentStrategy = new UtilityRentStrategy();
        }
        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
            if (OwnerId == null)
            {
                ctx.Bus.Publish(new LandUnownedProperty(this.TileId, this.Price));
                return;
            }
            else if (OwnerId != player.Id)
            {
                int amount = rentStrategy.CalculateRent(ctx, this, lastDiceSum);
                ctx.Bus.Publish(new RentDue(player.Id, OwnerId.Value, amount, this.TileId));
                return;
            }
        }
    }
}
