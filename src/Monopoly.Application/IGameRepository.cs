using Monopoly.Domain.Core;
using Monopoly.Domain.Factory;

namespace Monopoly.Application.Ports; 

public interface IGameRepository
// nơi lưu game
{
    void Save(GameSnapshot snapshot);
    // lưu trạng thái game
    GameSnapshot? Load(string slot);
    //GameSnapshot? có dữ liệu -> snapshot, k -> null ==> tải trạng thái game dựa trên slot
}
public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, Board board, int CurrentPlayerIndex)
{
    public GameSnapshot(string Slot, IReadOnlyList<Player> Players, int CurrentPlayerIndex)
        : this(Slot, Players, new Board(TileFactory.CreateFromConfig()), CurrentPlayerIndex) { }
}
// Lưu thông tin game tại 1 thời điểm
