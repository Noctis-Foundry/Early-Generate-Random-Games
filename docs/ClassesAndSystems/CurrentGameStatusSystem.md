# Current Game Status System

## Overview
Real-time game session tracking system displaying current game information with automatic database synchronization. Uses MVVM pattern with Avalonia UI, PostgreSQL listener for live updates, and timer-based countdown display.

## Purpose
- Display active game session details with real-time updates
- Show countdown timer until game completion deadline
- Track game progress and completion status
- Provide game finishing workflow with screenshot and comment submission
- Automatically sync with database changes via PostgreSQL LISTEN/NOTIFY

## Components

### CurrentGameStatusViewModel.cs (ViewModel)
ViewModel managing game status with real-time updates and timer functionality.

**Namespace**: `GameRandom.ViewModels.BaseClasses`

**Injected Dependencies**:
- `DatabaseService` - Database operations
- `FinishedGameDialogService` - Game completion dialog
- `PostgresListener` - Real-time database change notifications
- `ErrorService` - Error message display
- `SteamService` - Image loading from bytes

**Key Properties**:
- `CurrentTime` (TimeSpan) - Remaining time until game deadline
- `AppInfo` (GameProgresses) - Current game information
- `ImageBitmap` (Bitmap) - Game header image
- `UserGame` (UserGame) - User's game state
- `IsEmpty` (bool) - Indicates if no active game

**Core Methods**:

#### LoadInfo() → Task
Loads current user's game information and initializes timer.

**Flow**:
1. Acquires semaphore lock (1 second timeout)
2. Retrieves UserGame from database by user ID
3. Loads GameProgresses data by AppId
4. Converts game header image from bytes to Bitmap
5. Starts countdown timer if game found
6. Sets IsEmpty = false on success

**Error Handling**: Logs errors, releases semaphore in finally block

#### FinishingGame() → Task
Initiates game completion workflow.

**Flow**:
1. Acquires finish semaphore lock
2. Validates AppInfo is not null
3. Opens FinishedGameDialogService for screenshot/comment
4. Updates UserGame in database (moves to next game or clears)
5. Clears content if no more games in queue

**Database Update Logic**:
- If AppIdList has games: Set AppId to first item, remove from list
- If AppIdList empty: Set AppId to 0 (no active game)

#### ClearingContent()
Resets view to empty state.

**Actions**:
- Loads empty GameProgresses
- Sets default placeholder image
- Stops and unsubscribes timer
- Clears UserGame reference

**Private Helper Methods**:

#### InitializeAppInfo(int appId) → Task<bool>
Loads game data from database by AppId.

**Returns**: true if game found, false otherwise

#### GetUserGameFromUserId(ulong steamId) → Task<UserGame>
Retrieves UserGame entity for specified Steam ID.

**Throws**: NullReferenceException if not found

#### PostgresLoadInfo(PayloadStructure payload) → Task
Handles database change notifications.

**Logic**:
1. Retrieves updated UserGame by RowId
2. Validates it belongs to current user
3. Calls LoadInfo() to refresh display

#### StartTimer()
Initializes DispatcherTimer for countdown display.

**Configuration**:
- Interval: 1000ms (1 second)
- Handler: UpdateDateTimer()
- Stops existing timer before starting new one

#### UpdateDateTimer()
Updates CurrentTime property with remaining time.

**Calculation**: `AppInfo.EndTime - DateTime.Now`

## Real-Time Synchronization

### PostgreSQL Listener Integration

**Subscription**: Subscribes to `UserGames` table changes

**Event Handler**:
```csharp
_listener += structure =>
{
    if (structure.TableCode == (int)TableEnum.UserGames)
    {
        Dispatcher.UIThread.InvokeAsync(async () => await PostgresLoadInfo(structure));
    }
};

_postgresListener.Subscribe(TableEnum.UserGames, _listener);
```

**Behavior**:
- Listens for INSERT/UPDATE/DELETE on UserGames table
- Automatically refreshes display when user's game changes
- Ensures UI stays synchronized across multiple clients
- Uses UI thread dispatcher for thread-safe updates

## Timer System

### Countdown Display

**Implementation**: DispatcherTimer with 1-second interval

**Purpose**: Shows remaining time until game completion deadline

**Update Logic**:
```csharp
private void UpdateDateTimer()
{
    if (AppInfo is not null)
        CurrentTime = _appInfo.EndTime - DateTime.Now;
}
```

**Lifecycle**:
- Started when game info loaded successfully
- Stopped when game finished or content cleared
- Properly disposed to prevent memory leaks

## Data Flow

### Initial Load
```
ViewModel.LoadInfo() → GetUserGameFromUserId() → InitializeAppInfo() → StartTimer() → UI Update
```

### Real-Time Updates
```
Database Change → PostgresListener → PostgresLoadInfo() → LoadInfo() → UI Refresh
```

### Game Completion
```
FinishingGame() → FinishedGameDialogService → ChangeUserGame() → Database Update → ClearingContent()
```

## Usage Example

### Initialization
```csharp
var viewModel = new CurrentGameStatusViewModel();
await viewModel.LoadInfo();
```

### Binding in XAML
```xml
<TextBlock Text="{Binding AppInfo.AppName}"/>
<TextBlock Text="{Binding AppInfo.BeginTime, StringFormat='Start: {0:dd.MM.yyyy}'}"/>
<TextBlock Text="{Binding CurrentTime, StringFormat='Time left: {0:dd}d {0:hh}h {0:mm}m'}"/>
<Image Source="{Binding ImageBitmap}"/>
<Button Command="{Binding FinishingGame}" Content="Finish Game"/>
```

### Manual Refresh
```csharp
await viewModel.LoadInfo();
```

### Finishing Game
```csharp
await viewModel.FinishingGame();
// Opens dialog for screenshot and comment
// Updates database automatically
// Clears display if no more games
```

## Integration Points

### Database Service
- `GetUserGameAsync(ulong steamId, CancellationToken)` - Retrieves user's current game
- `GetFirstOrDefaultAsync<GameProgresses>(predicate, CancellationToken)` - Loads game details
- `UpdateAsync(UserGame, CancellationToken)` - Updates game queue
- `GetFromRowId<UserGame>(int rowId, CancellationToken)` - Loads entity by row ID

### FinishedGameDialogService
Opens modal dialog for game completion:
- Screenshot upload
- Comment submission
- Creates FinishedGames database entry

### PostgresListener
Real-time database change notifications:
- Subscribes to UserGames table
- Triggers automatic UI refresh
- Ensures multi-client synchronization

### SteamService
Image processing:
- `GetImageSyncFromBytes(byte[])` - Converts game header bytes to Bitmap

## Thread Safety

### Semaphores

**_loadInfoSemaphore** (SemaphoreSlim 1,1)
- Prevents concurrent LoadInfo() calls
- 1-second timeout on acquisition
- Released in finally block

**_finishSemaphore** (SemaphoreSlim 1,1)
- Prevents concurrent FinishingGame() calls
- Shows "Processing" message if locked
- Released in finally block

### UI Thread Dispatching
PostgreSQL listener callbacks use `Dispatcher.UIThread.InvokeAsync()` for thread-safe UI updates.

## Database Entities

### GameProgresses
```csharp
public class GameProgresses
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string AppName { get; set; }
    public byte[] AppHeaderImage { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime FinishTime { get; set; }
    public bool IsFinished { get; set; }
    public string Comment { get; set; }
    public int Grade { get; set; }
    public ulong PlayerId { get; set; }
}
```

### UserGame
```csharp
public class UserGame
{
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public int AppId { get; set; }  // Current active game
    public List<int>? AppIdList { get; set; }  // Queue of pending games
}
```

## Features

- **Real-Time Updates**: PostgreSQL LISTEN/NOTIFY integration
- **Countdown Timer**: Live display of remaining time
- **Thread-Safe**: Semaphore-based concurrency control
- **Automatic Sync**: Multi-client database synchronization
- **Game Queue**: Automatic progression to next game
- **Completion Workflow**: Integrated screenshot and comment submission
- **MVVM Pattern**: Clean separation of concerns
- **DI Integration**: Fully injected dependencies
- **Error Handling**: Comprehensive try-catch with logging
- **Memory Management**: Proper disposal of timers and resources

## Error Handling

- Semaphore timeout logging
- Database operation failures logged
- Null reference validation
- User-friendly error messages via ErrorService
- Graceful degradation on missing data
