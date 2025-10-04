using System;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Cards
{
    /// <summary>
    /// Cấp 1 thẻ "Get Out of Jail". Không dùng ngay.
    /// </summary>
    public sealed class GetOutOfJailCard : Card
    {
        public GetOutOfJailCard(string title, string description, Guid? tileId = null)
            : base(title, description, tileId ?? Guid.NewGuid()) { }

        public override void Resolve(GameContext ctx, IDomainEventBus bus, Player player)
        {
            player.GrantJailCard();
            // 2) Phát event cấp thẻ
            bus.Publish(new GotOutOfJailCardGranted(
                player.Id,
                player.HasJailCard   // số lượng hiện có sau khi tăng
            ));

            // 3) Kết thúc: CardResolved (audit)
            bus.Publish(new CardResolved(
                Title,
                player.Id,
                player.Cash,
                player.Position,
                $"Granted Get Out of Jail (now {player.HasJailCard})"
            ));
        }
    }
}
