using System;
using Monopoly.Application.UseCases;       
using Monopoly.Application.Ports;
using Monopoly.Domain.Events;

namespace Monopoly.ConsoleApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Monopoly Console Runner starting...");

            IGameRepository repo = new InMemoryGameRepository();
            IDomainEventBus domainEventBus = new InMemoryDomainEventBus();
            var useCase = new NewGameUseCase(repo, domainEventBus);

            var req = new NewGameRequest
            {
                Slot = "slot-1",
                PlayerNames = new[] { "Alice", "Bob" }
            };

            var res = useCase.Execute(req);
            Console.WriteLine($"NewGameUseCase executed. PlayerCount = {res.PlayerCount}");

            Console.WriteLine("Done.");
        }
    }
}