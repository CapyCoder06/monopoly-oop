using Monopoly.Domain.Core;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Factory;
using System.Collections.Generic;

namespace Monopoly.Application.Ports
{
    public static class GameSnapshotFactory
    {
        public static GameSnapshot CreateDefault(
            string slot,
            IReadOnlyList<Player> players,
            int currentPlayerIndex,
            IWallet wallet,
            Board? board = null
        )
        {
            var chanceDeck = new Deck<Card>(new List<Card>());
            var communityDeck = new Deck<Card>(new List<Card>());
            board ??= TileFactory.LoadFromJson("Data/BoardConfig.json", chanceDeck, communityDeck);

            return new GameSnapshot(slot, players, board, currentPlayerIndex, chanceDeck, communityDeck, wallet);
        }
    }
}
