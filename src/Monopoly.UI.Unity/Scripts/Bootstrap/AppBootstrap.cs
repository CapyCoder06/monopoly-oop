using Monopoly.Application.Ports;
using Monopoly.Application.UseCases;

namespace Monopoly.UI.Unity.Scripts.Bootstrap;

public class AppBootstrap
{
    private readonly IGameRepository _repo;
    private readonly IUiEventBus _ui;

    public AppBootstrap(IGameRepository repo, IUiEventBus ui)
    {
        _repo = repo; _ui = ui;
    }

    public void Start()
    {
        var newGame = new NewGameUseCase(_repo);
        newGame.Execute(new NewGameRequest {
            Slot = "default",
            PlayerNames = new[] { "Alice", "Bob" }
        });
    }
}
