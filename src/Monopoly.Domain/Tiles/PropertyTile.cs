using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Strategy;
using System;



namespace Monopoly.Domain.Tiles;

public enum PropertyColor
{
    Brown, LightBlue, Pink, Orange, Red, Yellow, Green, DarkBlue
}
public class PropertyTile : Tile
{
    public int Price { get; } 
    public Guid? OwnerId { get; set; }
    public PropertyColor Color { get; set; }
    public int BaseRent {get;}
    public IRentStrategy rentStrategy;
    public PropertyTile(int index, string name, PropertyColor color, int price, int baseRent, Guid? tileId = null)
        : base(index, name, tileId ?? Guid.NewGuid())
    {
        Price = price;
        Color = color;
        BaseRent = baseRent;
        rentStrategy = new PropertyRentStrategy(BaseRent);
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
