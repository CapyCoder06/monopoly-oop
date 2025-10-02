using Monopoly.Domain.Abstractions; // Card base
using Monopoly.Domain.Core;         // Deck, GameContext, Player
using Monopoly.Domain.Events;       // CardDrawn
using Monopoly.Domain.Tiles;

namespace Monopoly.Domain.Tiles
{
    public sealed class ChanceTile : Tile
    {
        private readonly Deck<Card> _deck;

        // Điều chỉnh ctor cho khớp base Tile của bạn (index/name/tileId có thể khác)
        public ChanceTile(int index, Deck<Card> deck, string name = "Chance", Guid? tileId = null)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
            _deck = deck;
        }

        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
            // 1) Rút thẻ (deck xoay vòng)
            var card = _deck.Draw();

            // 2) Publish CardDrawn (TileId, PlayerId, CardTitle)
            ctx.Bus.Publish(new CardDrawn(this.TileId, player.Id, card.Title));

            // 3) Resolve thẻ (thẻ sẽ tự publish FundsChanged / MovedByCard / CardResolved)
            card.Resolve(ctx, ctx.Bus, player);
        }
    }
}
