using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Application.DTO;

namespace Monopoly.Application.UseCases;
public class LoadGameUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;

    public LoadGameUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }

    public GameSnapshot? Execute(string slot)
    {
        var snap = _repo.Load(slot);
        if (snap == null)
        {
            _ui.Publish($"No save in {slot}");
            return null;
        }
        _ui.Publish($"Loaded {slot}");
        return snap;
    }
}
