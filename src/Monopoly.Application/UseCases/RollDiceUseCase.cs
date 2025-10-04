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
    public RollDiceUseCase(IGameRepository repo, IUiEventBus ui, TurnManager turnManager, IDomainEventBus domainEventBus)
    {
        _repo = repo;
        _ui = ui;
        _turnManager = turnManager;
        _domainEventBus = domainEventBus;
    }

    public void Execute(string slot)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException($"No game found in slot {slot}");
        var player = snapshot.Players[snapshot.CurrentPlayerIndex];
        var wallet = new InMemoryWallet(_domainEventBus);
        var newSnapshot = new GameSnapshot(Slot: slot, Players: snapshot.Players, CurrentPlayerIndex: snapshot.CurrentPlayerIndex, wallet: snapshot.Wallet);
        var ctx = new GameContext(newSnapshot.board, _domainEventBus, wallet);
        var tile = newSnapshot.board.Tiles[player.Position];
        var (sum, isDouble) = _turnManager.RollDiceAndAdvance(player, ctx);
        _repo.Save(newSnapshot);
        _ui.Publish(new PlayerMovedUiEvent(player.Id, player.Position, sum, isDouble));
    }
}
