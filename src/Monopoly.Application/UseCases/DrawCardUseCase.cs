using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Events;

namespace Monopoly.Application.UseCases;

public class DrawCardUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;
    private readonly IDomainEventBus _domainEventBus;
    public DrawCardUseCase(IGameRepository repo, IUiEventBus ui, IDomainEventBus _domainEventBus)
    {
        _repo = repo;
        _ui = ui;
        _domainEventBus = _domainEventBus;
    }
    public void Execute(string slot, string deckType, Guid playerID)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException("No game found");
        var player = snapshot.Players.First(p => p.Id == playerID);
        var wallet = new InMemoryWallet(_domainEventBus);
        var ctx = new GameContext(snapshot.board, _domainEventBus, wallet);
        Deck<Card> deck = deckType switch
        {
            "Chance" => snapshot.ChanceDeck,
            "CommunityChest" => snapshot.CommunityChestDeck,
            _ => throw new ArgumentException("Invalid deck type")
        };
        var card = deck.Draw();
        if (card == null) throw new InvalidOperationException("Deck is empty");
        var currentTile = snapshot.board.Tiles[player.Position];

        _ui.Publish(new CardDrawn(currentTile.TileId, playerID, card.Title));

        card.Resolve(ctx, _domainEventBus, player);

        _ui.Publish(new CardResolved(card.Title, playerID,player.Cash,player.Position, card.Title));
    }
}
