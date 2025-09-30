using Monopoly.Domain.Core;
using Monopoly.Application.Ports;

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
        var tile = game.Tiles[tileIndex];
        var payer = game.Players.First(p => p.Id == payerId);

        if (tile.OwnerId == null)
            throw new InvalidOperationException("Tile has no owner.");

        var owner = game.Players.First(p => p.Id == tile.OwnerId);

        payer.Pay(amount);
        owner.Receive(amount);

        _ui.Publish($"{payer.Name} paid {amount}$ rent to {owner.Name}");

        _repo.Save(game);
    }
}
