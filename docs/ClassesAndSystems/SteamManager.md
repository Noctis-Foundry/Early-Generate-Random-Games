# SteamManager

## Overview
Singleton manager for Steamworks API initialization, lifecycle management, and callback processing. Handles Steam client integration and user identification.

## Purpose
- Initialize and shutdown Steamworks API
- Process Steam callbacks via timer
- Retrieve Steam user ID
- Singleton access pattern

## Pattern
Lazy-initialized thread-safe singleton.

## Properties

### _isInitialized (private)
Tracks Steam API initialization state.

### _steamCallbackTimer (private)
DispatcherTimer for processing Steam callbacks every 10ms.

## Constants

**MaxTryToConnect** = 6 (currently unused)

## Methods

### InitSteam()
Initializes Steamworks API and starts callback processing.

**Process**:
1. Check if already initialized (early return)
2. Call SteamAPI.Init()
3. Start callback timer
4. Set initialized flag

**Error Handling**:
- Logs error and rethrows exception on failure
- Prevents double initialization

**Example**:
```csharp
var manager = SteamManager.GetSteamManager();
manager.InitSteam();
```

### StartEventTimer() (private)
Creates and starts DispatcherTimer for Steam callback processing.

**Configuration**:
- Interval: 10ms
- Action: SteamAPI.RunCallbacks()

**Purpose**: Processes Steam events (achievements, callbacks, etc.)

### ShutdownSteam()
Cleanly shuts down Steamworks API.

**Process**:
1. Check if initialized (early return if not)
2. Stop callback timer
3. Call SteamAPI.Shutdown()
4. Reset initialized flag
5. Log shutdown completion

**Example**:
```csharp
manager.ShutdownSteam();
```

### GetSteamId()
Retrieves current user's Steam ID.

**Returns**: `CSteamID` - Steam user identifier

**Throws**: Exception if Steam not initialized

**Example**:
```csharp
CSteamID steamId = manager.GetSteamId();
Console.WriteLine($"Steam ID: {steamId.m_SteamID}");
```

### GetSteamManager() (static)
Returns singleton instance.

**Returns**: `SteamManager` - Singleton instance

**Throws**: Exception if instance is null

**Example**:
```csharp
var manager = SteamManager.GetSteamManager();
```

### GetSteamIdAsLong() (static)
Convenience method to get Steam ID as ulong.

**Returns**: `ulong` - Steam ID as 64-bit integer

**Example**:
```csharp
ulong steamId = SteamManager.GetSteamIdAsLong();
```

## Usage Example

### Application Lifecycle
```csharp
// Startup
var steamManager = SteamManager.GetSteamManager();
try
{
    steamManager.InitSteam();
    Logger.Info("Steam initialized");
}
catch (Exception ex)
{
    Logger.Error($"Steam init failed: {ex.Message}");
    return;
}

// Runtime
ulong userId = SteamManager.GetSteamIdAsLong();
var user = await dbService.GetUserByUlongId(userId);

// Shutdown
steamManager.ShutdownSteam();
```

### Integration with User System
```csharp
public async Task<Users?> GetCurrentUser()
{
    ulong steamId = SteamManager.GetSteamIdAsLong();
    return await dbService.GetUserByUlongId(steamId);
}
```

## Callback Processing

Steam callbacks are processed every 10ms via DispatcherTimer:
- Friend requests
- Lobby invites
- Achievement unlocks
- Overlay events
- Network messages

**Critical**: Must call SteamAPI.RunCallbacks() regularly for Steam features to work.

## Initialization Requirements

**Prerequisites**:
- Steam client must be running
- Application must be launched through Steam or have steam_appid.txt
- Valid Steam App ID

**Failure Scenarios**:
- Steam client not running
- Invalid App ID
- Missing Steamworks binaries
- Already initialized by another instance

## Singleton Pattern

### Thread Safety
Uses Lazy<T> for thread-safe lazy initialization.

### Access
```csharp
// Correct
var manager = SteamManager.GetSteamManager();

// Don't create directly
// var manager = new SteamManager(); // ❌ Constructor is private
```

## Features

- **Lazy Initialization**: Instance created on first access
- **Thread-Safe**: Lazy<T> ensures single instance
- **Automatic Callbacks**: Timer-based callback processing
- **Clean Shutdown**: Proper resource cleanup
- **Error Handling**: Logs and rethrows initialization errors

## Best Practices

1. **Initialize Early**: Call InitSteam() at application startup
2. **Shutdown on Exit**: Call ShutdownSteam() before application close
3. **Check Initialization**: Verify Steam initialized before using features
4. **Handle Failures**: Catch initialization exceptions gracefully
5. **Single Instance**: Always use GetSteamManager()

## Limitations

- No retry logic for initialization
- MaxTryToConnect constant unused
- No connection state monitoring
- Timer interval not configurable
- No async initialization support

## Dependencies

- Steamworks.NET
- Avalonia.Threading (DispatcherTimer)
- Logger service

## Potential Improvements

```csharp
public async Task<bool> InitSteamAsync(int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            if (SteamAPI.Init())
            {
                StartEventTimer();
                _isInitialized = true;
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Steam init attempt {i + 1} failed: {ex.Message}");
            await Task.Delay(1000);
        }
    }
    
    Logger.Error("Steam initialization failed after all retries");
    return false;
}

public bool IsInitialized => _isInitialized;
```
