using Monopoly.Application.Ports;

namespace Monopoly.UI.Unity.Scripts.Mapping;

public class UiEventBus : IUiEventBus
{
    public event Action<object>? OnUiEvent;
    public void Publish(object notification) => OnUiEvent?.Invoke(notification);
}
