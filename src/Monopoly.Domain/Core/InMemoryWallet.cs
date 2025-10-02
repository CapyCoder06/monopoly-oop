using System;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Core
{
    public sealed class InMemoryWallet : IWallet
    {
        private readonly IDomainEventBus _bus;
        public InMemoryWallet(IDomainEventBus bus) => _bus = bus;

        public void Debit(Player player, int amount, string reason)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount must be positive");
            player.Cash -= amount;
            _bus.Publish(new FundsChanged(player.Id, -amount, reason, player.Cash));
        }

        public void Credit(Player player, int amount, string reason)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount must be positive");
            player.Cash += amount;
            _bus.Publish(new FundsChanged(player.Id, +amount, reason, player.Cash));
        }
        }
}
