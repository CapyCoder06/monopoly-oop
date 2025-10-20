using Monopoly.Application.Ports;
using Monopoly.Domain.Core;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Events;
using System.Linq;

namespace Monopoly.Application.UseCases;

public class NewGameRequest
{

    public required string Slot { get; init; }
    public required string[] PlayerNames { get; init; }
    public int StartingCash { get; init; } = 1500;
}

public class NewGameResponse
{
    public required int PlayerCount { get; init; }
}

public class NewGameUseCase
{
    private readonly IGameRepository _repo;
    private readonly IDomainEventBus _domainEventBus;

    public NewGameUseCase(IGameRepository repo, IDomainEventBus domainEventBus)
    {
        _repo = repo;
        _domainEventBus = domainEventBus;
    }
    public NewGameResponse Execute(NewGameRequest req)
    {
        var wallet = new InMemoryWallet(_domainEventBus);
        var players = req.PlayerNames.Select(n => new Player(n, req.StartingCash)).ToList();

        var snapshot = GameSnapshotFactory.CreateDefault(
            req.Slot,
            players,
            0,
            wallet
        );

        _repo.Save(snapshot);
        return new NewGameResponse { PlayerCount = players.Count };
    }
}
