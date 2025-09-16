using Monopoly.Domain.Core;

namespace Monopoly.Application.Ports;

public interface IGameRepository
{
    void Save(GameSnapshot snapshot);
    GameSnapshot? Load(string slot);
}

public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, int CurrentPlayerIndex);
