using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Application.DTO;

namespace Monopoly.Application.UseCases;

public class SaveGameUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;

    public SaveGameUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }

    public void Execute(GameSnapshot snapshot)
    {
        _repo.Save(snapshot);
        _ui.Publish($"Game saved to slot {snapshot.Slot}");
    }
}