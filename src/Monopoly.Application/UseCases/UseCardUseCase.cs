using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Application.DTO;
using Monopoly.Domain.State;

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
        if (player.CurrentState is not IPlayerState State)
            throw new InvalidOperationException("Player is not in jail");
        if (!player.TryConsumeJailCard())
            throw new InvalidOperationException("Player does not have a Get Out of Jail Free card");
        player.CurrentState = new NormalState();
        _repo.Save(snapshot);
        _ui.Publish(new ToastVM($"{player.Name} used a Get Out of Jail Free card", "info"));
    }
}
