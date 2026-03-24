using GameRandom.Scr.Events;
using Xunit;

namespace GameRandom.UnitTests.Services;

public class EventBusTests
{
    private readonly EventBus _eventBus = new();

    [Fact]
    public void Subscribe_And_Publish_Should_InvokeHandler()
    {
        // Arrange
        string? receivedData = null;
        void Handler(string data) => receivedData = data;

        // Act
        _eventBus.Subscribe<string>(Handler);
        _eventBus.Publish("test data");

        // Assert
        Assert.Equal("test data", receivedData);
    }

    [Fact]
    public void Unsubscribe_Should_StopHandlerFromInvoking()
    {
        // Arrange
        int callCount = 0;
        void Handler(int data) => callCount++;
        _eventBus.Subscribe<int>(Handler);

        // Act
        _eventBus.Publish(1);
        _eventBus.Unsubscribe<int>(Handler);
        _eventBus.Publish(2);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void ClearAll_Should_RemoveAllSubscriptions()
    {
        // Arrange
        int callCount = 0;
        void Handler(int data) => callCount++;
        _eventBus.Subscribe<int>(Handler);

        // Act
        _eventBus.ClearAll();
        _eventBus.Publish(1);

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Publish_MultipleHandlers_Should_InvokeAll()
    {
        // Arrange
        int callCount1 = 0;
        int callCount2 = 0;
        _eventBus.Subscribe<int>(_ => callCount1++);
        _eventBus.Subscribe<int>(_ => callCount2++);

        // Act
        _eventBus.Publish(42);

        // Assert
        Assert.Equal(1, callCount1);
        Assert.Equal(1, callCount2);
    }
}
