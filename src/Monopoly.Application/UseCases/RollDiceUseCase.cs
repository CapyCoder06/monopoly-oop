using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Application.DTO;
namespace Monopoly.Application.UseCases;

public class RollDiceUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;
    private readonly TurnManager _turnManager;
    public RollDiceUseCase(IGameRepository repo, IUiEventBus ui, TurnManager turnManager)
    {
        _repo = repo;
        _ui = ui;
        _turnManager = turnManager;
    }

    public void Execute(string slot)
    {
        var snapshot = _repo.Load(slot);
        if (snapshot is null)
            throw new InvalidOperationException($"No game found in slot {slot}");
        var player = snapshot.Players[snapshot.CurrentPlayerIndex];
        var (sum, isDouble) = _turnManager.RollDiceAndAdvance(player);
        var newSnapshot = new GameSnapshot(Slot: slot,Players: snapshot.Players,CurrentPlayerIndex: snapshot.CurrentPlayerIndex);
        _repo.Save(newSnapshot);
        _ui.Publish(new PlayerMovedUiEvent(player.Id,player.Position,sum,isDouble));
    }
}
