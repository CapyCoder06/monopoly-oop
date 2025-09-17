using Monopoly.Application.Ports;
using Monopoly.Application.UseCases;

namespace Monopoly.UI.Unity.Scripts.Controllers;

public class GameController
{
    // readonly: file chỉ xuất dữ liệu, không nhập.
    private readonly RollDiceUseCase _roll;

    public GameController(IGameRepository repo, IUiEventBus ui)
    {
        _roll = new RollDiceUseCase(repo, ui);
    }

    public void OnRollButtonClicked() => _roll.Execute();
}
