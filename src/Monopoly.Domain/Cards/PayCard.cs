using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Events;
using Monopoly.Domain.Core;

namespace Monopoly.Domain.Cards;

public class PayCard : Card {
    public int Amount { get;}
    public PayCard(string title, string description, int amount)
        : base(title, description)
    {
        Amount = amount;
    }
    public override void Resolve(GameContext ctx, IDomainEventBus bus, Player player)
    {
        ctx.Wallet.Debit(player, Amount, Title);
        bus.Publish(new CardResolved(Title, player.Id, player.Cash, player.Position, $"Pay {Amount}"));
    }
}