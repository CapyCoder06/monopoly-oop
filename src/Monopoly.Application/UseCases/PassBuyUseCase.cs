using Monopoly.Application.Ports;

namespace Monopoly.Application.UseCases;

public class PassBuyUseCase
{
    private readonly IUiEventBus _ui;

    public PassBuyUseCase(IUiEventBus ui)
    {
        _ui = ui;
    }

    public void Execute(int tileIndex, int playerId)
    {
        _ui.Publish($"Player {playerId} passed buying tile {tileIndex}");
    }
}
