# EventBus

## Overview
Lightweight publish-subscribe event system for decoupled communication between components. Enables type-safe event handling without direct dependencies.

## Purpose
- Decouple event publishers from subscribers
- Type-safe event handling
- Dynamic subscription management
- Centralized event distribution

## Architecture

**Storage**: Dictionary mapping event types to delegate handlers

**Pattern**: Observer/Pub-Sub pattern

## Methods

### Subscribe<T>(Action<T> handler)
Registers handler for specific event type.

**Type Parameter**:
- `T` - Event data type

**Parameters**:
- `handler` - Callback invoked when event published

**Behavior**:
- First subscription creates new entry
- Subsequent subscriptions combine delegates
- Multiple handlers can subscribe to same event type

**Example**:
```csharp
eventBus.Subscribe<LobbyUpdate>(data => 
{
    Console.WriteLine($"Lobby updated: {data.MemberCount}");
});
```

### Unsubscribe<T>(Action<T> handler)
Removes specific handler from event type.

**Type Parameter**:
- `T` - Event data type

**Parameters**:
- `handler` - Handler to remove

**Behavior**:
- Removes handler from delegate chain
- Deletes entry if no handlers remain
- Safe to call with non-existent handler

**Example**:
```csharp
eventBus.Unsubscribe<LobbyUpdate>(myHandler);
```

### Publish<T>(T eventData)
Publishes event to all subscribed handlers.

**Type Parameter**:
- `T` - Event data type

**Parameters**:
- `eventData` - Event payload

**Behavior**:
- Invokes all registered handlers for type T
- Handlers execute synchronously in registration order
- No-op if no subscribers exist

**Example**:
```csharp
eventBus.Publish(new LobbyUpdate(lobbyData));
```

### ClearAll()
Removes all event subscriptions.

**Usage**: Cleanup during shutdown or reset

## Usage Patterns

### Basic Pub-Sub
```csharp
// Subscribe
eventBus.Subscribe<GameStarted>(data => 
{
    LoadGame(data.GameId);
});

// Publish
eventBus.Publish(new GameStarted { GameId = 123 });
```

### Multiple Subscribers
```csharp
// UI updates
eventBus.Subscribe<PlayerJoined>(data => UpdatePlayerList(data));

// Logging
eventBus.Subscribe<PlayerJoined>(data => Logger.Info($"Player joined: {data.Name}"));

// Analytics
eventBus.Subscribe<PlayerJoined>(data => TrackEvent(data));

// All three handlers invoked on publish
eventBus.Publish(new PlayerJoined { Name = "Player1" });
```

### Cleanup
```csharp
public class MyService : IDisposable
{
    private Action<MyEvent> _handler;
    
    public MyService(EventBus eventBus)
    {
        _handler = OnMyEvent;
        eventBus.Subscribe(_handler);
    }
    
    public void Dispose()
    {
        eventBus.Unsubscribe(_handler);
    }
    
    private void OnMyEvent(MyEvent data) { }
}
```

## Event Examples

### LobbyUpdate Event
```csharp
public class LobbyUpdate
{
    public List<LobbyData> Members { get; }
    
    public LobbyUpdate(List<LobbyData> members)
    {
        Members = members;
    }
}

// Usage
eventBus.Subscribe<LobbyUpdate>(update => 
{
    foreach (var member in update.Members)
    {
        DisplayMember(member);
    }
});
```

### GameStateChanged Event
```csharp
public record GameStateChanged(GameState OldState, GameState NewState);

eventBus.Publish(new GameStateChanged(GameState.Menu, GameState.Playing));
```

## Features

- **Type Safety**: Compile-time type checking for events
- **Flexible**: Any type can be used as event
- **Lightweight**: Minimal overhead, simple implementation
- **Thread-Unsafe**: Not designed for concurrent access
- **Synchronous**: Handlers execute immediately on publish

## Best Practices

1. **Use Immutable Events**: Prefer records or readonly properties
2. **Unsubscribe on Cleanup**: Prevent memory leaks
3. **Keep Handlers Fast**: Avoid blocking operations
4. **Descriptive Event Names**: Clear intent (e.g., LobbyUpdate, PlayerJoined)
5. **Single Responsibility**: One event type per logical event

## Integration with DI

```csharp
// Register as singleton
factory.Create<EventBus>(new EventBus());

// Inject into services
public class LobbyService
{
    [Inject] private readonly EventBus _eventBus = null!;
    
    public void UpdateLobby()
    {
        _eventBus.Publish(new LobbyUpdate(members));
    }
}
```

## Limitations

- Not thread-safe (requires external synchronization)
- Synchronous execution (no async handlers)
- No event priority or ordering control
- No exception isolation between handlers
- Memory leaks if subscriptions not cleaned up
