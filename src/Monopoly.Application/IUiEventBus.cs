namespace Monopoly.Application.Ports;

public interface IUiEventBus
{
    void Publish(object notification);
    // Gửi thông báo cho các thành phần khác trong hệ thống tiếp nhận và phản ứng
}
