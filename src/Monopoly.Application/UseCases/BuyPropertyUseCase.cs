using Monopoly.Domain.Core; 
using Monopoly.Application.Ports; 

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
        var tile = game.Tiles[tileIndex];
        var buyer = game.Players.First(p => p.Id == buyerId);

        if (tile.OwnerId != null)
            throw new InvalidOperationException("Tile already owned.");

        if (buyer.Cash < tile.Price)
            throw new InvalidOperationException("Not enough cash.");

        tile.OwnerId = buyer.Id;
        buyer.Pay(tile.Price);

        _ui.Publish($"{buyer.Name} bought {tile.Name} for {tile.Price}$");

        _repo.Save(game);
    }
}
