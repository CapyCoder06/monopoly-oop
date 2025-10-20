namespace Monopoly.Application.Ports;

public interface IUiEventBus
{
    void Publish(object notification);

}
