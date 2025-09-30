using Monopoly.Domain.Core;

namespace Monopoly.Application.Ports
{
public interface IGameRepository
// nơi lưu game
{
    void Save(GameSnapshot snapshot);
    // lưu trạng thái game
    GameSnapshot? Load(string slot);
    //GameSnapshot? có dữ liệu -> snapshot, k -> null ==> tải trạng thái game dựa trên slot
}

public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, int CurrentPlayerIndex);
// Lưu thông tin game tại 1 thời điểm
}
