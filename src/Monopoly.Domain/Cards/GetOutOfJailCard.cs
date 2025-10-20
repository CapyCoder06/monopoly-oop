using System;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Cards
{

    public sealed class GetOutOfJailCard : Card
    {
        public GetOutOfJailCard(string title, string description, Guid? tileId = null)
            : base(title, description, tileId ?? Guid.NewGuid()) { }

        public override void Resolve(GameContext ctx, IDomainEventBus bus, Player player)
        {
            player.GrantJailCard();
            bus.Publish(new GotOutOfJailCardGranted(
                player.Id,
                player.HasJailCard  
            ));

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
