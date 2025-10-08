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
    private readonly IDomainEventBus _domainEventBus;
    private readonly IDice _dice;
    public RollDiceUseCase(IGameRepository repo, IUiEventBus ui, TurnManager turnManager, IDomainEventBus domainEventBus, IDice dice)
    {
        _repo = repo;
        _ui = ui;
        _turnManager = turnManager;
        _domainEventBus = domainEventBus;
        _dice = dice;
    }

    public void Execute(string slot)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException($"No game found in slot {slot}");
        var player = snapshot.Players[snapshot.CurrentPlayerIndex];
        var ctx = new GameContext(snapshot.board, _domainEventBus, snapshot.Wallet);
        var (sum, isDouble) = _turnManager.RollDiceAndAdvance(player, ctx);
        _repo.Save(snapshot);
        _ui.Publish(new PlayerMovedUiEvent(player.Id, player.Position, sum, isDouble));
    }
}
