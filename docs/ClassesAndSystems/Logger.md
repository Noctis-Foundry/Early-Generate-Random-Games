# Logger

## Overview
Static console logging utility with color-coded severity levels. Provides simple debugging and diagnostic output.

## Purpose
- Color-coded console output
- Severity-based logging
- Quick debugging during development

## Methods

### Info(string msg)
Logs informational messages in green.

**Parameters**:
- `msg` - Message to log

**Color**: Green

**Usage**: Success messages, confirmations, general info

**Example**:
```csharp
Logger.Info("User connected successfully");
```

### Error(string msg)
Logs error messages in red.

**Parameters**:
- `msg` - Error message

**Color**: Red

**Usage**: Exceptions, failures, critical issues

**Example**:
```csharp
Logger.Error("Failed to connect to database");
```

### Warning(string msg)
Logs warning messages in yellow.

**Parameters**:
- `msg` - Warning message

**Color**: Yellow

**Usage**: Non-critical issues, deprecations, potential problems

**Example**:
```csharp
Logger.Warning("Connection timeout, retrying...");
```

### Debug(string msg)
Logs debug messages in gray.

**Parameters**:
- `msg` - Debug message

**Color**: Gray

**Usage**: Detailed diagnostic information, trace logs

**Example**:
```csharp
Logger.Debug($"Processing item {itemId}");
```

### Message()
Resets console color to white and prints newline.

**Color**: White

**Usage**: Reset formatting after colored output

## Usage Examples

### Basic Logging
```csharp
Logger.Info("Application started");
Logger.Debug("Loading configuration...");
Logger.Warning("Config file not found, using defaults");
Logger.Error("Failed to initialize Steam API");
```

### Error Handling
```csharp
try
{
    await ConnectToDatabase();
    Logger.Info("Database connected");
}
catch (Exception ex)
{
    Logger.Error($"Database error: {ex.Message}");
}
```

### Debug Tracing
```csharp
Logger.Debug($"User {userId} joined lobby {lobbyId}");
Logger.Debug($"Lobby members: {memberCount}");
```

## Color Reference

| Method  | Color  | Console Color      |
|---------|--------|--------------------|
| Info    | Green  | ConsoleColor.Green |
| Error   | Red    | ConsoleColor.Red   |
| Warning | Yellow | ConsoleColor.Yellow|
| Debug   | Gray   | ConsoleColor.Gray  |
| Message | White  | ConsoleColor.White |

## Features

- **Simple API**: Static methods, no instantiation needed
- **Color Coding**: Visual severity distinction
- **Lightweight**: Direct console output, no overhead
- **Cross-Platform**: Works on Windows, Linux, macOS

## Limitations

- Console output only (no file logging)
- No log levels filtering
- No timestamps
- No structured logging
- Not thread-safe
- No log rotation or persistence

## Future Improvements (TODO)

Comment in code indicates planned integration:
```csharp
// To:Do Connect Error window to logger
```

Planned features:
- GUI error window integration
- Persistent log files
- Log level configuration
- Structured logging support

## Best Practices

1. **Use Appropriate Levels**: Match severity to method
2. **Include Context**: Add relevant IDs, names, values
3. **Avoid Sensitive Data**: Don't log passwords, tokens
4. **Keep Messages Concise**: Clear, actionable information

## Integration Example

```csharp
public class DatabaseService
{
    public async Task<bool> AddItemAsync<T>(T item)
    {
        try
        {
            await db.AddAsync(item);
            Logger.Debug($"Added {item} to db");
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to add item: {e.Message}");
            return false;
        }
    }
}
```
