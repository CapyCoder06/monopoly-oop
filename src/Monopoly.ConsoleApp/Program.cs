using System;
using Monopoly.Application.UseCases;       // NewGameUseCase, NewGameRequest
using Monopoly.Application.Ports;         // IGameRepository

namespace Monopoly.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Monopoly Console Runner starting...");

        IGameRepository repo = new InMemoryGameRepository();
        var useCase = new NewGameUseCase(repo);

        // NewGameRequest có required members: Slot, PlayerNames
        var req = new NewGameRequest
        {
            Slot = "slot-1",
            PlayerNames = new[] { "Alice", "Bob" }
            // StartingCash giữ mặc định 1500; cần thì set thêm
            // StartingCash = 1500
        };

        var res = useCase.Execute(req);
        Console.WriteLine($"NewGameUseCase executed. PlayerCount = {res.PlayerCount}");

        Console.WriteLine("Done.");
    }
}
