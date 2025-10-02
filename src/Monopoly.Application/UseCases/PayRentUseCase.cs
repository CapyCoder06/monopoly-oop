using Monopoly.Domain.Core;
using Monopoly.Application.Ports;
using Monopoly.Domain.Tiles;

namespace Monopoly.Application.UseCases;

public class PayRentUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;

    public PayRentUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }

    public void Execute(string slot, int tileIndex, Guid payerId, int amount)
    {
        var game = _repo.Load(slot);
        var property = game.board.Tiles[tileIndex] as PropertyTile ?? throw new InvalidOperationException("Tile is not a property.");
        var payer = game.Players.First(p => p.Id == payerId);

        if (property.OwnerId == null)
            throw new InvalidOperationException("Tile has no owner.");

        var owner = game.Players.First(p => p.Id == property.OwnerId);

        payer.Pay(amount);
        owner.Receive(amount);

        _ui.Publish($"{payer.Name} paid {amount}$ rent to {owner.Name}");

        _repo.Save(game);
    }
}
