using Monopoly.Application.Ports;

namespace Monopoly.Application.UseCases;

public class RollDiceUseCase(IGameRepository repo, IUiEventBus ui)
{
    public void Execute()
    {
        // Mốc 1 sẽ nối với Domain.TurnManager; giữ stub để compile
        ui.Publish(new { Type = "RollRequested" });
    }
}
