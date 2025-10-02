using Monopoly.Application.DTO;
using Monopoly.Application.Ports;
using Monopoly.Application.UseCases;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;

namespace Monopoly.UI.Unity.Scripts.Controllers;

public class GameController
{
    // readonly: file chỉ xuất dữ liệu, không nhập.
    private readonly RollDiceUseCase _roll;
    private string _currentSlot;

    public GameController(IGameRepository repo, IUiEventBus ui, TurnManager turnManager, IDomainEventBus domainEventBus)
    {
        _roll = new RollDiceUseCase(repo, ui, turnManager, domainEventBus);
    }
    public void SetSlot(string slot) => _currentSlot = slot;
    public void OnRollButtonClicked() => _roll.Execute(_currentSlot);
}
