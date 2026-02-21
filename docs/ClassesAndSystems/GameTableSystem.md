# Game Table System

## Overview
UserControl displaying real-time game progress table for all players in lobby. Features manga-style theming, automatic updates via PostgresListener, and lobby-based filtering.

## Purpose
- Display game progress for all lobby members
- Real-time updates via database notifications
- Filter games by lobby membership
- Show completion status and dates
- Manga-inspired visual design

## Layout

### Background
- RandomGame.png image (80% opacity)
- Black overlay (60% opacity)
- Manga aesthetic

### Structure

**Header** (Border):
- Title: "GAME PROGRESS TABLE"
- Font size: 28px, Bold, White
- Close button (top-right)
- Black background with white borders

**Column Headers** (Border):
- PLAYER (150px width)
- GAME (150px width)
- STATUS (150px width)
- START DATE (150px width)
- END DATE (150px width)
- Black background with white borders

**Content Area** (ScrollViewer):
- Height: 300px
- Scrollable ListBox
- Semi-transparent black background
- White borders between rows

### Data Display

Each row shows:
- **PlayerID** - Steam user ID
- **AppName** - Game title
- **IsFinished** - Completion status (converted via BoolConverter)
- **BeginTime** - Start date
- **EndTime** - End date

## Code-behind (GameTable.axaml.cs)

### Injected Dependencies
- `_databaseService` (DatabaseService) - Data operations
- `_converter` (ObservableConverter) - Collection conversion
- `_errorService` (ErrorService) - Error display

### Fields
- `_cts` (CancellationTokenSource) - Async cancellation
- `_savedDelegate` (Action<PayloadStructure>) - Database event handler
- `_onShowContent` (Action<string>) - Navigation callback

### Constructor

**Process**:
1. Initialize component
2. Resolve DI dependencies
3. Create GameTableViewModel
4. Skip if design mode
5. Start InitializeTable() task
6. Create database event delegate
7. Subscribe to PostgresListener (GameTable events)
8. Trigger initial table update

### Methods

**InitializeTable()**
Loads initial game progress data.

**Process**:
1. Query all GameProgresses from database
2. Update table on UI thread
3. Handle errors via ErrorService

**AddListener(Action<string> onChangeContent)**
Registers navigation callback.

**Close(object? sender, RoutedEventArgs e)**
Navigates to "Main" page.

**SubscribeToUpdateTable(int tableCode)**
Handles database change notifications.

**Process**:
1. Validate table code (must be GameTable)
2. Get current user info
3. Load all game progresses
4. Filter by lobby membership:
   - If no lobby (ID ≤ 0): Show only user's games
   - If in lobby: Show all lobby members' games (TODO)
5. Update table with filtered data

**Lobby Filtering Logic**:
```csharp
if (userInfo.LobbyID <= 0)
    finallyList = gameList.Where(x => x.PlayerID == userInfo.SteamID).ToList();
else
{
    // Get lobby members
    Lobbies? lobbies = await _databaseService.GetLobbyById(userInfo.LobbyID);
    
    // TODO: Filter by lobby members
    // Currently shows only user's games
    finallyList = gameList.Where(x => x.PlayerID == userInfo.SteamID).ToList();
}
```

**UpdateTable(List<GameProgresses> gameProgress)**
Updates ViewModel with new data.

**Dispose()**
Cleanup on control disposal.

**Process**:
1. Null navigation callback
2. Unsubscribe from PostgresListener
3. Null saved delegate
4. Null injected services
5. Cancel async tasks
6. Dispose CancellationTokenSource
7. Force garbage collection

## ViewModel

### GameTableViewModel

**Properties**:
- `GameProgress` (ObservableCollection<GameProgresses>) - Bound to ListBox

**Data Binding**:
```xml
<ListBox ItemsSource="{Binding GameProgress}">
    <DataTemplate>
        <TextBlock Text="{Binding AppName}"/>
        <TextBlock Text="{Binding IsFinished, Converter={StaticResource BoolConverter}}"/>
    </DataTemplate>
</ListBox>
```

## BoolConverter

### Purpose
Converts boolean IsFinished to display text.

**Conversion**:
- `true` → "Finished" or "✓"
- `false` → "In Progress" or "✗"

## Real-Time Updates

### Database Notification Flow
```
Database change → PostgresListener → EventBus → 
SubscribeToUpdateTable() → Filter by lobby → 
UpdateTable() → UI refresh
```

### Subscription
```csharp
listener.Subscribe(TableEnum.GameTable, payload =>
{
    Dispatcher.UIThread.InvokeAsync(() => 
        SubscribeToUpdateTable(payload.TableCode));
});
```

## Features

- **Real-Time Updates**: Automatic refresh on database changes
- **Lobby Filtering**: Shows relevant games based on lobby
- **Scrollable**: Handles large game lists
- **Manga Theme**: Black/white aesthetic with borders
- **Error Handling**: Comprehensive error display
- **Resource Cleanup**: Proper disposal pattern
- **Async Loading**: Non-blocking initialization

## Integration Example

```csharp
var factory = new UserControlFactory();
var gameTable = factory.CreateUserControl<GameTable>(pageName =>
{
    NavigateToPage(pageName);
});

gameTable.Open();
contentArea.Content = gameTable;
```

## Workflow

### Initial Load
```
Constructor → InitializeTable() → Load all games →
Filter by user → Update UI
```

### Database Update
```
Game added/updated → PostgresListener notification →
SubscribeToUpdateTable() → Reload games → Filter → Update UI
```

### Lobby Join
```
User joins lobby → LobbyID updated → Next table update →
Filter by lobby members → Show all lobby games
```

## Limitations

- Lobby filtering not fully implemented (TODO)
- Currently shows only user's games even in lobby
- No sorting options
- No search/filter UI
- No pagination for large datasets
- Aggressive garbage collection (may impact performance)
- No column resizing
- Fixed column widths

## Potential Improvements

```csharp
// Implement lobby filtering
if (userInfo.LobbyID > 0)
{
    var lobbies = await _databaseService.GetLobbyById(userInfo.LobbyID);
    var memberIds = lobbies.LobbyData.Select(ld => ld.UserId).ToList();
    
    finallyList = gameList
        .Where(x => memberIds.Contains(x.PlayerID))
        .ToList();
}

// Add sorting
public void SortByColumn(string columnName, bool ascending)
{
    var sorted = ascending
        ? GameProgress.OrderBy(g => GetPropertyValue(g, columnName))
        : GameProgress.OrderByDescending(g => GetPropertyValue(g, columnName));
    
    GameProgress = new ObservableCollection<GameProgresses>(sorted);
}

// Add filtering
public void FilterByStatus(bool? isFinished)
{
    var filtered = _allGames.Where(g => 
        !isFinished.HasValue || g.IsFinished == isFinished.Value);
    
    GameProgress = new ObservableCollection<GameProgresses>(filtered);
}

// Add pagination
public void LoadPage(int pageNumber, int pageSize)
{
    var page = _allGames
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    
    GameProgress = new ObservableCollection<GameProgresses>(page);
}
```

## Testing

```csharp
[Test]
public async Task TestTableLoading()
{
    var gameTable = new GameTable();
    await Task.Delay(1000); // Wait for initialization
    
    var vm = gameTable.DataContext as GameTableViewModel;
    Assert.IsNotNull(vm.GameProgress);
    Assert.IsTrue(vm.GameProgress.Count > 0);
}

[Test]
public void TestLobbyFiltering()
{
    // Test filtering logic
    var userGames = games.Where(g => g.PlayerID == userId).ToList();
    Assert.AreEqual(expectedCount, userGames.Count);
}
```
