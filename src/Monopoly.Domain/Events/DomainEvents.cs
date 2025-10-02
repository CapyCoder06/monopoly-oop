using System;
using System.Collections.Generic;
using System.Linq;
using Monopoly.Domain.Core;

namespace Monopoly.Domain.Events
{
    // Hợp đồng (contract) chung cho mọi Domain Event trong hệ thống.
    // Bất kỳ sự kiện nào cũng phải có thời điểm xảy ra (OccurredAt).
    // Dùng interface để tách "định nghĩa" khỏi "cách thực thi" → dễ mở rộng, dễ test.
    public interface IDomainEvent
    {
        // Thời điểm sự kiện xảy ra (theo UTC để tránh vấn đề múi giờ giữa các máy khác nhau).
        DateTime OccurredAt { get; }
    }

    /*
    record = dạng class bất biến (immutable-by-default), tối ưu cho việc mang dữ liệu (data carrier).
    Guid = định danh duy nhất toàn cục (128-bit) → nhận diện Player không nhầm lẫn.
    PlayerMoved mô tả: "Người chơi đã di chuyển từ From đến To".
    Event này là 'thông điệp' để các phần khác (UI, logging, rule engine) có thể phản ứng.
     */
    public record PlayerMoved(Guid PlayerId, int From, int To) : IDomainEvent
    {
        // Dấu thời gian UTC của sự kiện khi được tạo.
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    // Kênh phát (publish) và lấy (dequeue) các Domain Event.
    // Interface giúp lớp cấp cao (UseCase/TurnManager) không phụ thuộc vào triển khai cụ thể
    // (nguyên lý Dependency Inversion – SOLID).
    public interface IDomainEventBus
    {
        // Đăng (publish) một sự kiện vào bus.
        // Dùng '@event' vì 'event' là từ khóa C#.
        void Publish(IDomainEvent @event);

        // Lấy ra toàn bộ sự kiện đã publish và đồng thời xóa chúng khỏi hàng đợi.
        // Trả về IReadOnlyList để bên ngoài không thể chỉnh sửa danh sách.
        IReadOnlyList<IDomainEvent> DequeueAll();
    }

    // Triển khai đơn giản lưu sự kiện trong bộ nhớ (in-memory).
    // Phù hợp cho unit test / prototype; khi triển khai thực tế có thể thay bằng
    // Message Broker (RabbitMQ/Kafka) hoặc lưu DB để audit.
    public class InMemoryDomainEventBus : IDomainEventBus
    {
        // Danh sách lưu trữ tạm thời các event đã publish.
        // 'readonly' để không thể thay đổi tham chiếu _events sau khi khởi tạo (an toàn hơn về mặt thiết kế).
        private readonly List<IDomainEvent> _events = new();
        // Thêm event vào hàng đợi nội bộ.
        public void Publish(IDomainEvent @event) => _events.Add(@event);

        // Trả về một bản sao (copy) của danh sách events, rồi xóa danh sách gốc.
        // - Dùng ToList() để tách dữ liệu trả về khỏi vùng lưu trữ nội bộ (tránh bị chỉnh sửa từ bên ngoài).
        // - Clear() để "dequeue" (mô phỏng hành vi lấy-ra-khỏi-hàng).
        public IReadOnlyList<IDomainEvent> DequeueAll()
        {
            var copy = _events.ToList(); // copy dữ liệu hiện có
            _events.Clear();             // xóa hàng đợi để chuẩn bị cho các sự kiện mới
            return copy;                 // trả về snapshot chỉ-đọc
        }
    }
    public record LandUnownedProperty(Guid TileId, int Price) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record RentDue(Guid PlayerId, Guid OwnerId, int Amount, Guid TileId) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    //Jail
    public record WentToJail(Guid PlayerId, int JailIndex) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public enum JailLeaveReason
    {
        RolledDouble,
        UsedCard,
        BailPaid,
        AfterThreeTurns
    }
    public record LeftJail(Guid PlayerId, JailLeaveReason Reason) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record BailPaid(Guid PlayerId, int Amount) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record UsedJailCard(Guid PlayerId) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record RolledDoubleToLeave(Guid PlayerId, int Sumdice) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public record FundsChanged(Guid PlayerId, int Amount, string Reason, int NewCash) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record CardDrawn(Guid TileId, Guid PlayerId, string CardTitle) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public record CardResolved(string Title, Guid PlayerId, int AfterCash, int AfterPosition, string Effect) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record MovedByCard(Guid PlayerId, int From, int To, string Reason, bool PassedGo, int GoBonus) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public record GotOutOfJailCardGranted(Guid PlayerId, int CountInHand) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
