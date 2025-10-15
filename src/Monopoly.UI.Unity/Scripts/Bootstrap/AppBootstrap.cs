using Monopoly.Application.Ports;
using Monopoly.Application.UseCases;
using Monopoly.Domain.Events;

namespace Monopoly.UI.Unity.Scripts.Bootstrap;

public class AppBootstrap
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;
    private readonly IDomainEventBus _domainEventBus;

    public AppBootstrap(IGameRepository repo, IUiEventBus ui, IDomainEventBus domainEventBus)
    {
        _repo = repo; _ui = ui; _domainEventBus = domainEventBus;
    }

    public void Start()
    {
        var newGame = new NewGameUseCase(_repo, _domainEventBus);
        newGame.Execute(new NewGameRequest {
            Slot = "default",
            PlayerNames = new[] { "Alice", "Bob" }
        });
    }
}
