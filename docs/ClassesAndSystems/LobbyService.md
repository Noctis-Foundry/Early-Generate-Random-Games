# LobbyService

## Overview
`LobbyService` manages multiplayer game sessions by grouping players into lobbies. This filtering system ensures players only see information relevant to their current group, preventing clutter when playing with friends.

## Purpose
Lobbies filter players into separate groups, allowing:
- Isolated game sessions for friend groups
- Clean UI with only relevant player information
- Top panel avatar display showing current lobby members
- Event-driven updates via EventBus (LobbyUpdate event)

## Dependencies
- `DatabaseService` - Database operations
- `EventBus` - Publishes LobbyUpdate events
- `ErrorService` - Error handling and user notifications
- `User` - Current user information

## Constants
- `EmptyLobbyId = 0` - No lobby assigned
- `DisconnectedLobbyId = -1` - User disconnected state

## Core Methods

### StartApp()
Initializes application by loading current user's lobby and sending initial LobbyUpdate event.

**Flow**:
1. Get current user
2. Find user's lobby by ID
3. Publish LobbyUpdate event

### CreateLobby()
Creates a new lobby for the current user.

**Flow**:
1. Check if lobby creation is already in progress
2. Get current user
3. Disconnect from existing lobby if needed
4. Generate unique lobby ID
5. Create lobby data entry
6. Update user's lobby ID
7. Save lobby to database
8. Publish LobbyUpdate event

**Error Handling**:
- Prevents concurrent lobby creation
- Rolls back user lobby ID on database failure

### ConnectToLobby(long lobbyId)
Connects user to an existing lobby.

**Parameters**:
- `lobbyId` - Target lobby identifier

**Flow**:
1. Validate lobby ID
2. Get current user
3. Disconnect from current lobby
4. Find target lobby
5. Add user to lobby members
6. Update member count
7. Save changes to database

**Validation**:
- Rejects EmptyLobbyId (0)
- Verifies lobby exists before connection

### DisconnectFromLobby()
Removes user from current lobby.

**Flow**:
1. Get current user
2. Find current lobby
3. Update user lobby ID to DisconnectedLobbyId (-1)
4. Remove user from lobby members list
5. Update member count
6. Delete lobby if empty, otherwise update database
7. Publish LobbyUpdate event if lobby still exists

**Cleanup**:
- Automatically deletes empty lobbies
- Notifies remaining members via LobbyUpdate event

## Private Helper Methods

### GetCurrentUserAsync()
Retrieves current user with null safety check.

### FindLobbyAsync(long lobbyId)
Fetches lobby from database by ID with error handling.

### SendLobbyEvent(Lobbies? lobbies)
Publishes LobbyUpdate event to EventBus with lobby member data.

### DisconnectIfInLobby(Users userInfo)
Disconnects user from current lobby if already in one.

### GenerateLobbyId()
Generates random unique lobby ID (1 to long.MaxValue).

### CreateLobbyData(Users userInfo, long lobbyId)
Creates LobbyData entry for user-lobby association.

## Event System

### LobbyUpdate Event
Published via EventBus when lobby state changes:
- User joins lobby
- User leaves lobby
- Lobby created
- Application starts

**Payload**: `List<LobbyData>` - Current lobby members

**Subscribers**: UI systems (avatar display panel, member lists)

## State Management
- `_isCreating` - Prevents concurrent lobby creation
- User lobby ID states:
  - `0` - No lobby
  - `-1` - Disconnected
  - `> 0` - Active lobby ID

## Usage Example
```csharp
// Create new lobby
await lobbyService.CreateLobby();

// Connect to friend's lobby
await lobbyService.ConnectToLobby(friendLobbyId);

// Leave lobby
await lobbyService.DisconnectFromLobby();
```

## UI Integration
Top panel displays lobby members using avatars, updated automatically via LobbyUpdate event subscriptions.

---

## LobbyWindow

### Purpose
Modal window providing UI for lobby creation and connection. Visualizes LobbyService operations with manga-style theming.

### Layout
- Size: 1000x550
- Background: RandomGame.png with black overlay (70% opacity)
- Centered on screen
- Title: "LOBBY CONNECTION"

### UI Structure

**Header**:
- Title: "LOBBY MANAGER"
- Black background with white bottom border
- Bold Arial font, size 24

**Create Lobby Section**:
- Title: "CREATE NEW LOBBY"
- Lobby ID display (bound to CurrentLobbyID)
- "CREATE LOBBY" button
- Black background with white borders

**Separator**: White horizontal line

**Connect Section**:
- Title: "CONNECT TO EXISTING LOBBY"
- Instruction text
- Lobby ID input (TextBox, max 20 digits)
- "CONNECT" button

### Code-behind (LobbyWindow.axaml.cs)

**Injected Dependencies**:
- `_eventBus` (EventBus) - Event system
- `_lobbyService` (LobbyService) - Lobby operations
- `_errorService` (ErrorService) - Error display

**Fields**:
- `MaxLenghtId = 18` - Maximum lobby ID length

**Constructor**:
1. Initialize component
2. Create CreateLobbyViewModel
3. Set DataContext
4. Register ViewModel in DI
5. Resolve dependencies
6. Validate injected services

**Methods**:

**OnLobbyIdChanging(object sender, TextChangedEventArgs e)**
- Filters input to digits only
- Validates lobby ID format
- Updates TextBox text

**Connect(object? sender, RoutedEventArgs e)**
- Parses lobby ID from IdBox
- Calls LobbyService.ConnectToLobby()
- Shows error if parsing fails

**Create(object? sender, RoutedEventArgs e)**
- Calls LobbyService.CreateLobby()
- Updates CurrentLobbyID via ViewModel

### ViewModel Integration

**CreateLobbyViewModel**:
- `CurrentLobbyID` property - Displays created lobby ID
- Bound to lobby ID label
- Updated after lobby creation

### Workflow

**Create Lobby**:
```
User clicks "CREATE LOBBY" → LobbyService.CreateLobby() →
Generate ID → Save to DB → Update ViewModel.CurrentLobbyID →
UI displays new lobby ID
```

**Connect to Lobby**:
```
User enters lobby ID → Clicks "CONNECT" → Parse ID →
LobbyService.ConnectToLobby(id) → Validate lobby →
Add user to lobby → Update DB → Publish LobbyUpdate event
```

### Features

- **Input Validation**: Digits-only lobby ID
- **Visual Feedback**: Displays created lobby ID
- **Error Handling**: Shows errors via ErrorService
- **Manga Theme**: Black/white aesthetic
- **DI Integration**: All services injected
- **Event-Driven**: Updates via EventBus

### Integration Example

```csharp
// Open from MainWindow menu
public void OpenLobbyWindow()
{
    var lobbyWindow = new LobbyWindow();
    lobbyWindow.ShowDialog(this);
}
```

### Limitations

- No lobby list/browser
- Manual ID entry required
- No validation of lobby existence before connect attempt
- No disconnect button
- Hardcoded max ID length
- No lobby member preview
