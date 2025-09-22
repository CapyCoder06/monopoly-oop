namespace Monopoly.Domain.Core;

//IDice: giao diện định nghĩa phương thức / hợp đồng (contract) Roll để quay xúc xắc.
//Nhờ đó, code ở chỗ khác (ví dụ TurnManager) chỉ cần quan tâm đến "tôi cần một thứ có thể Roll()", chứ không quan tâm nó dùng thuật toán gì để roll.
public interface IDice
{
    (int d1, int d2, int sum, bool isDouble) Roll();
}

//Dice: lớp triển khai giao diện IDice, sử dụng Random để tạo số ngẫu nhiên cho hai viên xúc xắc.
public class Dice : IDice
{
    private readonly Random _rng = new();
    public (int d1, int d2, int sum, bool isDouble) Roll()
    {
        var d1 = _rng.Next(1, 7);
        var d2 = _rng.Next(1, 7);
        return (d1, d2, d1 + d2, d1 == d2);
    }
}

