using Monopoly.Application.Ports;
using Monopoly.Domain.Core;

namespace Monopoly.Application.UseCases;

public class NewGameRequest
{
    //required: bắt buộc phải có giá trị khi khởi tạo object
    //init: chỉ gán giá trị lúc khởi tạo, không thay đổi sau đó
    public required string Slot { get; init; }
    public required string[] PlayerNames { get; init; }
    public int StartingCash { get; init; } = 1500;
}

public class NewGameResponse
{
    public required int PlayerCount { get; init; }
}

//repo: dependency (kho lưu trữ game)
public class NewGameUseCase
{
    private readonly IGameRepository _repo;

    public NewGameUseCase(IGameRepository repo)
    {
        _repo = repo;
    }
    public NewGameResponse Execute(NewGameRequest req)
    {
        //Duyệt qua danh sách tên người chơi. Tạo Player mới với tên n và tiền khởi điểm req.StartingCash. Trả về List<Player> players
        var players = req.PlayerNames.Select(n => new Player(n, req.StartingCash)).ToList();
        //Tạo ảnh chụp trạng thái game với mã game, danh sách người chơi, lượt chơi ban đầu và lưu vào repository(repo.Save)
        _repo.Save(new GameSnapshot(req.Slot, players, 0));
        //Trả về số người chơi trong game mới
        return new NewGameResponse { PlayerCount = players.Count };
    }
}
