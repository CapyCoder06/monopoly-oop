using Monopoly.Application.Ports;
using Monopoly.Application.UseCases;
using Monopoly.Domain.Core;
using Xunit;

public class NewGameUseCaseTests
{
    [Fact]
    public void Create_New_Game_Saves_Snapshot()
    {
        var repo = new InMemoryRepo();
        var uc = new NewGameUseCase(repo);

        var resp = uc.Execute(new NewGameRequest {
            Slot = "s1",
            PlayerNames = new[] { "A", "B" },
            StartingCash = 1500
        });

        Assert.Equal(2, resp.PlayerCount);
        Assert.NotNull(repo.Load("s1"));
        Assert.Equal(0, repo.Load("s1")!.CurrentPlayerIndex);
        Assert.All(repo.Load("s1")!.Players, p => Assert.IsType<Player>(p));
    }

    private class InMemoryRepo : IGameRepository
    {
        private readonly Dictionary<string, GameSnapshot> _db = new();
        public GameSnapshot? Load(string slot) => _db.TryGetValue(slot, out var s) ? s : null;
        public void Save(GameSnapshot snapshot) => _db[snapshot.Slot] = snapshot;
    }
}
