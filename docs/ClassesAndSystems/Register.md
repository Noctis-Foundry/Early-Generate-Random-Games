# Register<TKey, TValue>

## Overview
Generic key-value registry for storing and retrieving objects. Provides type-safe dictionary wrapper with validation.

## Purpose
- Type-safe object registration
- Key-value storage with validation
- Null safety checks
- Duplicate key handling

## Type Parameters

**TKey** - Key type (must be non-null)

**TValue** - Value type (must be non-null)

## Methods

### RegisterNewObject(TKey key, TValue value)
Registers object with specified key.

**Parameters**:
- `key` - Unique identifier
- `value` - Object to register

**Validation**:
- Throws ArgumentNullException if key is null
- Throws ArgumentNullException if value is null
- Logs message if key already exists (doesn't throw)

**Behavior**:
- Adds key-value pair if key doesn't exist
- Ignores registration if key exists (no overwrite)

**Example**:
```csharp
var register = new Register<string, GameService>();

register.RegisterNewObject("mainGame", gameService);
register.RegisterNewObject("mainGame", otherService); // Logs "Dictionary have this key"
```

### GetObjectFromRegister(TKey key, out TValue? tValue)
Retrieves object by key.

**Parameters**:
- `key` - Key to lookup
- `tValue` - Output parameter for retrieved value

**Returns**: `bool` - True if key found, false otherwise

**Validation**:
- Throws ArgumentNullException if key is null

**Example**:
```csharp
if (register.GetObjectFromRegister("mainGame", out var service))
{
    service.Start();
}
else
{
    Console.WriteLine("Service not found");
}
```

## Usage Examples

### Service Registry
```csharp
var serviceRegistry = new Register<Type, object>();

serviceRegistry.RegisterNewObject(typeof(IDatabase), databaseService);
serviceRegistry.RegisterNewObject(typeof(ILogger), loggerService);

if (serviceRegistry.GetObjectFromRegister(typeof(IDatabase), out var db))
{
    var database = (IDatabase)db;
    database.Connect();
}
```

### UI Component Registry
```csharp
var componentRegistry = new Register<string, UserControl>();

componentRegistry.RegisterNewObject("menu", menuControl);
componentRegistry.RegisterNewObject("settings", settingsControl);

if (componentRegistry.GetObjectFromRegister("menu", out var control))
{
    control.Show();
}
```

### Game Object Registry
```csharp
var gameObjects = new Register<int, GameObject>();

foreach (var obj in objects)
{
    gameObjects.RegisterNewObject(obj.Id, obj);
}

if (gameObjects.GetObjectFromRegister(playerId, out var player))
{
    player.Update();
}
```

## Features

- **Type Safety**: Generic constraints ensure type correctness
- **Null Safety**: Validates keys and values
- **Duplicate Protection**: Prevents accidental overwrites
- **Simple API**: Two-method interface
- **Out Parameter Pattern**: Standard .NET retrieval pattern

## Limitations

- No overwrite capability (must remove and re-add)
- No removal method
- No enumeration support
- No clear/reset method
- Logs to console instead of proper logging

## Comparison with Dictionary

### Register<TKey, TValue>
```csharp
register.RegisterNewObject(key, value);
if (register.GetObjectFromRegister(key, out var value))
{
    // Use value
}
```

### Dictionary<TKey, TValue>
```csharp
dictionary.TryAdd(key, value);
if (dictionary.TryGetValue(key, out var value))
{
    // Use value
}
```

**Differences**:
- Register throws on null keys/values
- Register logs duplicate keys instead of returning false
- Register provides semantic naming for registration pattern

## Best Practices

1. **Use Descriptive Keys**: Clear, meaningful identifiers
2. **Check Return Values**: Always verify GetObjectFromRegister result
3. **Handle Nulls**: Ensure keys and values are non-null
4. **Consider Alternatives**: Use DI container for service registration

## Potential Improvements

```csharp
// Add removal
public bool Unregister(TKey key);

// Add enumeration
public IEnumerable<TKey> GetKeys();

// Add overwrite option
public void RegisterNewObject(TKey key, TValue value, bool overwrite = false);

// Add clear
public void Clear();

// Better logging
public void RegisterNewObject(TKey key, TValue value)
{
    if (!_registerValues.TryAdd(key, value))
        Logger.Warning($"Key '{key}' already registered");
}
```

## Use Cases

- Service locator pattern
- Component registry
- Factory object storage
- Configuration management
- Plugin registration
