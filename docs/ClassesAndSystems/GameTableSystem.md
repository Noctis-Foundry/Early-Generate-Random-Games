# Game Table System

## Overview
Real-time game progress table displaying completion status for all players in current lobby. Features automatic updates via PostgresListener, lobby-based filtering, and abstract base class architecture.

## Purpose
- Display game progress for lobby members
- Real-time synchronization via database notifications
- Filter games by lobby membership
- Show player names, games, completion status, and dates
- Provide abstract base for table ViewModels

## ViewModel Architecture

### GameTableViewModel

**Namespace**: `GameRandom.ViewModels.AdminConfirmSystem`

**Inheritance**: Extends `AbstractTableWindowViewModel<GameTableData>`

**Type Parameter**: `GameTableData` - Row data model

### AbstractTableWindowViewModel<T>

**Base Class**: Provides common table functionality

**Properties**:
- `TableData` (ObservableCollection<T>) - Bound to UI
- `IsProcess` (bool) - Loading indicator
- `StartProcessing` (Action) - Processing event

**Injected Services** (protected):
- `_databaseService` (DatabaseService) - Database operations

**Abstract Methods**:
- `LoadData(Func<T, bool>? predicate)` - Must be implemented by derived classes

**Helper Methods**:
- `IsNotValidateInjectingData()` - Validates injected services

### Core Methods

#### LoadData(Func<GameTableData, bool>? predicate) → Task (override)
Loads game progress data for all players in current user's lobby.

**Parameters**:
- `predicate` - Optional filter (not used in current implementation)

**Process**:
1. Validates injected services via IsNotValidateInjectingData()
2. Gets current user info from User.GetInstance()
3. Sets IsProcess = true and invokes StartProcessing event
4. Retrieves lobby by user's LobbyId
5. Validates lobby exists and has members
6. Calls LoadGroupTableData() with lobby members
7. Converts result to ObservableCollection
8. Assigns to TableData property
9. Catches and logs exceptions
10. Sets IsProcess = false in finally block

**Early Exit Conditions**:
- Lobby is null
- LobbyData.Count <= 0

#### LoadGroupTableData(List<LobbyData> lobbyData) → Task<List<GameTableData>?>
Loads game progress for each lobby member.

**Parameters**:
- `lobbyData` - List of lobby members

**Returns**: List of GameTableData or null if no data

**Process**:
1. Creates empty GameTableData list
2. Iterates through each lobby member:
   - Calls `_databaseService.GetGameTableData(userId)`
   - Retrieves (user, gameProgresses) tuple
   - Skips if user or gameProgresses is null
   - Iterates through user's games:
     - Extracts nickname (or "---" if empty)
     - Creates GameTableData entry
     - Adds to result list
3. Returns complete list

**Nickname Handling**:
```csharp
var userNickName = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : "---";
```

## Data Models

### GameTableData

**Purpose**: Represents single row in game progress table

**Properties**:

#### PlayerName (string)
Player's display name.

**Default**: Empty string

**Fallback**: "---" if user nickname is null or empty

#### GameInfo (GameProgresses)
Complete game progress information.

**Type**: GameProgresses entity

**Contains**:
- AppId (int) - Steam application ID
- AppName (string) - Game title
- AppHeaderImage (byte[]) - Game cover image
- BeginTime (DateTime) - Start date
- EndTime (DateTime) - Expected completion date
- FinishTime (DateTime) - Actual completion date
- IsFinished (bool) - Completion status
- Comment (string) - User comment
- Grade (int) - User rating
- PlayerId (ulong) - Steam user ID

### LobbyData

**Purpose**: Represents lobby membership

**Properties**:
- UserId (ulong) - Steam user ID
- LobbyId (long) - Lobby identifier

### Users

**Relevant Properties**:
- SteamID (ulong) - User identifier
- Nickname (string) - Display name
- LobbyId (long) - Current lobby

## Data Flow

### Initial Load
```
ViewModel.LoadData()
  ↓
Get current user info
  ↓
Retrieve lobby by LobbyId
  ↓
LoadGroupTableData(lobby.LobbyData)
  ↓
For each lobby member:
  ↓
  GetGameTableData(userId)
  ↓
  Extract user and games
  ↓
  Create GameTableData entries
  ↓
Convert to ObservableCollection
  ↓
Assign to TableData property
  ↓
UI updates via data binding
```

### Lobby Filtering
```
User joins lobby
  ↓
User.LobbyId updated in database
  ↓
Next LoadData() call
  ↓
GetLobbyById(user.LobbyId)
  ↓
Load games for all lobby members
  ↓
Display in table
```

## Usage Example

### Basic Usage
```csharp
var viewModel = new GameTableViewModel();
await viewModel.LoadData();

// Access table data
foreach (var row in viewModel.TableData)
{
    Console.WriteLine($"{row.PlayerName}: {row.GameInfo.AppName} - {row.GameInfo.IsFinished}");
}
```

### UI Binding
```xml
<DataGrid ItemsSource="{Binding TableData}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Player" Binding="{Binding PlayerName}"/>
        <DataGridTextColumn Header="Game" Binding="{Binding GameInfo.AppName}"/>
        <DataGridCheckBoxColumn Header="Finished" Binding="{Binding GameInfo.IsFinished}"/>
        <DataGridTextColumn Header="Start" Binding="{Binding GameInfo.BeginTime, StringFormat='dd.MM.yyyy'}"/>
        <DataGridTextColumn Header="End" Binding="{Binding GameInfo.EndTime, StringFormat='dd.MM.yyyy'}"/>
    </DataGrid.Columns>
</DataGrid>
```

### Manual Refresh
```csharp
// Reload table data
await viewModel.LoadData();
```

## Integration Points

### DatabaseService

**GetLobbyById(long lobbyId) → Task<Lobbies?>**
Retrieves lobby with member list.

**GetGameTableData(ulong userId) → Task<(Users?, List<GameProgresses>?)>**
Returns user info and their game progress list.

**Returns**: Tuple of (Users, List<GameProgresses>)

### User Singleton

**User.GetInstance().GetUserInfo() → UserInfo**
Provides current user information including LobbyId.

### AbstractTableWindowViewModel<T>

Base class providing:
- TableData property for UI binding
- IsProcess loading indicator
- Service injection validation
- Common table functionality

## Error Handling

### Service Validation
```csharp
if (IsNotValidateInjectingData()) 
    throw new NullReferenceException();
```
- Validates DatabaseService injection
- Throws exception if not injected

### Null Handling
- Skips lobby members with null user data
- Skips lobby members with null game progress
- Returns empty list if no valid data

### Exception Catching
```csharp
try
{
    // Load data logic
}
catch (Exception e)
{
    Logger.Error(e.Message);
}
finally
{
    IsProcess = false;
}
```
- Catches all exceptions during load
- Logs error message
- Always resets IsProcess flag

### Early Exit
- Returns if lobby is null
- Returns if lobby has no members
- No exception thrown for empty lobbies
