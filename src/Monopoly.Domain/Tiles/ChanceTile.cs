using Monopoly.Domain.Abstractions; 
using Monopoly.Domain.Core;         
using Monopoly.Domain.Events;      

namespace Monopoly.Domain.Tiles
{
    public sealed class ChanceTile : Tile
    {
        private readonly Deck<Card> _deck;

        public ChanceTile(int index, Deck<Card> deck, string name = "Chance", Guid? tileId = null)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
            _deck = deck;
        }

        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
            var card = _deck.Draw();

            ctx.Bus.Publish(new CardDrawn(this.TileId, player.Id, card.Title));

            card.Resolve(ctx, ctx.Bus, player);
        }
    }
}
