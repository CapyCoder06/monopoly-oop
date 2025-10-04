using Monopoly.Application.Ports;
using Monopoly.Application.DTO;
using Monopoly.Domain.Core;

namespace Monopoly.Application.UseCases;

public class EndTurnUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;
    public EndTurnUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }

    public void Execute(string slot)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException("No game found");
        var nextIndex = (snapshot.CurrentPlayerIndex + 1) % snapshot.Players.Count;
        var newSnapshot = new GameSnapshot(Slot: slot, Players: snapshot.Players, CurrentPlayerIndex: nextIndex, wallet: snapshot.Wallet);
        _repo.Save(newSnapshot);
        var nextPlayer = snapshot.Players[nextIndex];
        _ui.Publish(new TurnEndedUiEvent(nextPlayer.Id));
    }
}
