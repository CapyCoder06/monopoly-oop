using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.State;
using Monopoly.Application.DTO;

namespace Monopoly.Application.UseCases;

public class PayBailUseCase
{
    private readonly IUiEventBus _ui;
    private readonly IGameRepository _repo;
    public PayBailUseCase(IUiEventBus ui, IGameRepository repo)
    {
        _repo = repo;
        _ui = ui;
    }
    public void Execute(string slot, Guid playerID)
    {
        var snapshot = _repo.Load(slot) ?? throw new InvalidOperationException("No game found");
        var player = snapshot.Players.First(p => p.Id == playerID);
        if (player.CurrentState is not InJailState)
            throw new InvalidOperationException("Player is not in Jail");
        if (player.Cash < 50) // config số tiền bail
            throw new InvalidOperationException("Not enough money to pay bail");
        player.Pay(50);
        player.CurrentState = new NormalState();
        _repo.Save(snapshot);
        _ui.Publish(new ToastVM($"{player.Name} paid bail and left jail", "info"));
    }
}