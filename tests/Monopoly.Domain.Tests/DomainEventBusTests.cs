using System;
using Xunit;
using Monopoly.Domain.Events;

namespace Monopoly.Domain.Tests;

public class DomainEventBusTests
{
    [Fact]
    public void Publish_Then_DequeueAll_Should_Return_One_Event()
    {
        // Arrange
        var bus = new InMemoryDomainEventBus();
        var e = new PlayerMoved(Guid.NewGuid(), 1, 3);

        // Act
        bus.Publish(e);
        var events = bus.DequeueAll();

        // Assert
        Assert.Single(events);
        Assert.IsType<PlayerMoved>(events[0]);
        Assert.Equal(1, ((PlayerMoved)events[0]).From);
        Assert.Equal(3, ((PlayerMoved)events[0]).To);
    }

    [Fact]
    public void DequeueAll_Should_Empty_The_Queue()
    {
        var bus = new InMemoryDomainEventBus();
        bus.Publish(new PlayerMoved(Guid.NewGuid(), 2, 5));

        _ = bus.DequeueAll();
        var after = bus.DequeueAll();

        Assert.Empty(after);
    }
}
