using Monopoly.Domain.Core;
using Monopoly.Domain.Factory;
using Monopoly.Domain.Abstractions;

namespace Monopoly.Application.Ports; 

public interface IGameRepository
// nơi lưu game
{
    void Save(GameSnapshot snapshot);
    // lưu trạng thái game
    GameSnapshot? Load(string slot);
    //GameSnapshot? có dữ liệu -> snapshot, k -> null ==> tải trạng thái game dựa trên slot
}
public record GameSnapshot(string Slot, IReadOnlyList<Player> Players, Board board, int CurrentPlayerIndex, Deck<Card> ChanceDeck, Deck<Card> CommunityChestDeck, IWallet Wallet)
{
    public GameSnapshot(string Slot, IReadOnlyList<Player> Players, int CurrentPlayerIndex, IWallet wallet)
    : this(Slot, Players, TileFactory.CreateFromConfig(), CurrentPlayerIndex, new Deck<Card>(new List<Card>()), new Deck<Card>(new List<Card>()), wallet)
    { }
}
// Lưu thông tin game tại 1 thời điểm
