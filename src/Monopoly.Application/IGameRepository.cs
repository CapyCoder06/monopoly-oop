using Monopoly.Domain.Core;
using Monopoly.Domain.Factory;
using Monopoly.Domain.Abstractions;
using System.Collections.Generic;

namespace Monopoly.Application.Ports; 

public interface IGameRepository
{
    void Save(GameSnapshot snapshot);
    GameSnapshot? Load(string slot);
}
public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, Board board, int CurrentPlayerIndex, Deck<Card> ChanceDeck, Deck<Card> CommunityChestDeck, IWallet Wallet)
{

}
