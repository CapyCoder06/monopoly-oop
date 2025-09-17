namespace Monopoly.Domain.Events;

//Một class có thể triển khai nhiều interface (đa kế thừa) -> Muốn định nghĩa chuẩn chung cho nhiều class khác nhau (tính trừu tượng)
public interface IDomainEvent
{
    //DataTime: kiểu dữ liệu lưu ngày giờ trong C#.
    //OccurredAt = thuộc tính (property) cho biết sự kiện đã xảy ra vào lúc nào.
    DateTime OccurredAt { get; }
}

/*
- record = class đặc biệt chuyên để lưu trữ dữ liệu, có sẵn so sánh theo giá trị, immutable mặc định và code ngắn gọn.
- Guid = số định danh 128-bit duy nhất toàn cầu -> dùng để định danh duy nhất cho các thực thể trong hệ thống.
*/
public record PlayerMoved(Guid PlayerId, int From, int To) : IDomainEvent
{
    //DateTime.UtcNow = lấy thời gian hiện tại theo chuẩn UTC (Coordinated Universal Time) -> tránh các vấn đề liên quan đến múi giờ.
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public interface IDomainEventBus
{
    /*
    Từ khóa @event: Vì event là từ khóa đặc biệt trong C#, nên thêm @ để có thể dùng nó như tên biến.
    Phương thức Publish: để đăng ký (publish) một sự kiện vào hệ thống.
    IReadOnlyList<IDomainEvent>: trả về một danh sách các sự kiện đã được đăng ký và đồng thời đảm bảo rằng danh sách này không thể bị thay đổi từ bên ngoài (chỉ đọc).
    DequeueAll: để lấy tất cả các sự kiện đã được đăng ký và đồng thời xóa chúng khỏi hệ thống (giống như lấy ra khỏi hàng đợi).
    */
    void Publish(IDomainEvent @event); 
    IReadOnlyList<IDomainEvent> DequeueAll();
}

public class InMemoryDomainEventBus : IDomainEventBus
{
    //readonly _list: danh sách chỉ đọc để lưu trữ các sự kiện, không thể thay đổi tham chiếu/gán sau khi khởi tạo.
    private readonly List<IDomainEvent> _events = new();
    public void Publish(IDomainEvent @event) => _events.Add(@event);
    public IReadOnlyList<IDomainEvent> DequeueAll()
    {
        //ToList(): tạo một bản sao mới của danh sách hiện tại.
        var copy = _events.ToList();
        _events.Clear();
        return copy;
    }
}
