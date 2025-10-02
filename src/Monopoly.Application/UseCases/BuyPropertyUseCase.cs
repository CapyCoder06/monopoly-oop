using Monopoly.Domain.Core;
using Monopoly.Application.Ports;
using Monopoly.Domain.Tiles;

namespace Monopoly.Application.UseCases;

public class BuyPropertyUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;

    public BuyPropertyUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }

    public void Execute(string slot,int tileIndex, Guid buyerId)
    {
        var game = _repo.Load(slot);
        var property = game.board.Tiles[tileIndex] as PropertyTile ?? throw new InvalidOperationException("Tile is not a property.");
        var buyer = game.Players.First(p => p.Id == buyerId);

        if (property.OwnerId != null)
            throw new InvalidOperationException("Tile already owned.");

        if (buyer.Cash < property.Price)
            throw new InvalidOperationException("Not enough cash.");

        property.OwnerId = buyer.Id;
        buyer.Pay(property.Price);

        _ui.Publish($"{buyer.Name} bought {property.Name} for {property.Price}$");

        _repo.Save(game);
    }
}
