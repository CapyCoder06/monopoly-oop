using Monopoly.Domain.Core;

namespace Monopoly.Domain.Abstractions;

public interface IWallet
{
    void Debit(Player player, int amount, string reason);
    void Credit(Player player, int amount, string reason);
}