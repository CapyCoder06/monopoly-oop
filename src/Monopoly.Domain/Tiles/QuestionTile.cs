using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Abstractions;
using System;


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
            var card = _deck.Draw();

            ctx.Bus.Publish(new CardDrawn(
                this.TileId,
                player.Id,
                card.Title
            ));

            card.Resolve(ctx, ctx.Bus, player);
        }
    }
}
