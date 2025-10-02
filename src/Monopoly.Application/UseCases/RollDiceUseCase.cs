using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Application.DTO;
using Monopoly.Domain.State;

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
        var newSnapshot = new GameSnapshot(Slot: slot, Players: snapshot.Players, CurrentPlayerIndex: snapshot.CurrentPlayerIndex);
        var ctx = new GameContext(newSnapshot.board, _domainEventBus);
        var tile = newSnapshot.board.Tiles[player.Position];
        var (sum, isDouble, d1, d2) = _turnManager.RollDiceAndAdvance(player, ctx);
        if (player.CurrentState is InJailState jailState)
        {
            if (isDouble)
            {
                player.CurrentState = new NormalState();
                player.Move(sum, snapshot.board.Tiles.Count);
                _ui.Publish(new ToastVM($"{player.Name} rolled doubles and left jail!", "success"));
            }
            else
            {
                bool canMove = jailState.OnRollDice(player, ctx, d1, d2, JailReleaseAction.None);
                if (jailState.TurnsLeft == 0)
                {
                    if (player.Cash >= 50)
                    {
                        player.Pay(50);
                        player.CurrentState = new NormalState();
                        _ui.Publish(new ToastVM($"{player.Name} auto-paid bail and left jail", "info"));
                    }
                    else
                    {

                    }
                }
                else
                {
                    _ui.Publish(new ToastVM($"{player.Name} failed to roll doubles. Turns left in jail: {jailState.TurnsLeft}", "warn"));
                }
            }
            _repo.Save(snapshot);
            return;
        }
        tile.OnLand(ctx, player, sum);
        _repo.Save(newSnapshot);
        _ui.Publish(new PlayerMovedUiEvent(player.Id, player.Position, sum, isDouble));
    }
}
