using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.Application.UseCases;

public class UseCardUseCase
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;
    public UseCardUseCase(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo;
        _ui = ui;
    }
    public void Execute(string slot, Guid playerId)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException("No game found");
        var player = snapshot.Players.First(p => p.Id == playerId);
        if (player.State is not InJailState jailState)
            throw new InvalidOperationException("Player is not in jail");
        if (!player.HasJailCard)
            throw new InvalidOperationException("Player does not have a Get Out of Jail Free card");
        player.HasJailCard = false;
        player.State = new NormalState();
        _repo.Save(snapshot);
        _ui.Publish(new ToastVM($"{player.Name} used a Get Out of Jail Free card", "info"));
    }
}
