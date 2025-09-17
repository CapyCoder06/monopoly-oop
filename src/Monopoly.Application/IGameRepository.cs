using Monopoly.Domain.Core;

namespace Monopoly.Application.Ports;

public interface IGameRepository
// nơi lưu game
{
    void Save(GameSnapshot snapshot);
    //lưu trạng thái game
    GameSnapshot? Load(string slot);
    //Dùng để tải lại trạng thái game dựa trên slot (một khóa định danh, ví dụ "Save1", "Save2")
    // Trả về GameSnapshot? (nullable record): Nếu có dữ liệu → trả về snapshot. Nếu không có dữ liệu → trả về null. 
}

public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, int CurrentPlayerIndex);
// Lưu thông tin game tại 1 thời điểm
