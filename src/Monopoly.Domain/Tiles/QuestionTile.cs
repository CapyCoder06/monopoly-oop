using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Abstractions;

namespace Monopoly.Domain.Tiles
{
    public class QuestionTile : Tile
    {
        private readonly Deck<Card> _deck;

        public QuestionTile(int index, Deck<Card> deck, string name = "Question", Guid? tileId = null)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
        }

        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
            // 1. Bốc thẻ
            var card = _deck.Draw();

            // 2. Event: CardDrawn
            ctx.Bus.Publish(new CardDrawn(
                this.TileId,
                player.Id,
                card.Title
            ));

            // 3. Resolve thẻ
            card.Resolve(ctx, ctx.Bus, player);
        }
    }
}
