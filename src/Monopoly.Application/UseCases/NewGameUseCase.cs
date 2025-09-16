using Monopoly.Application.Ports;
using Monopoly.Domain.Core;

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

public class NewGameUseCase(IGameRepository repo)
{
    public NewGameResponse Execute(NewGameRequest req)
    {
        var players = req.PlayerNames.Select(n => new Player(n, req.StartingCash)).ToList();
        repo.Save(new GameSnapshot(req.Slot, players, 0));
        return new NewGameResponse { PlayerCount = players.Count };
    }
}
