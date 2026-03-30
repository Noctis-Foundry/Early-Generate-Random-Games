# Lobby System

## Overview
Lobby management system for multiplayer game sessions. Groups players into isolated lobbies with automatic member tracking and event-driven updates.

## Components

### LobbyService
Core service managing lobby operations and state.

### LobbyWindowViewModel
ViewModel for lobby UI displaying current lobby ID with real-time updates.

## LobbyWindowViewModel

**Namespace**: `GameRandom.ViewModels.AdminConfirmSystem`

**Inheritance**: Extends `ViewModelBase`

### Purpose
Manages lobby window UI state and displays current user's lobby ID.

### Properties

#### CurrentLobbyID (long)
Displays current user's lobby identifier.

**Binding**: Two-way data binding with UI

**Default**: 0 (DefaultIdMessage)

**Updates**: Automatically via EventBus subscription

### Constants

**DefaultIdMessage = 0**
Displayed when user has no active lobby.

### Constructor

**Process**:
1. Checks if in design mode (returns early if true)
2. Calls GetCurrentId() to initialize lobby ID
3. Subscribes to EventBus for LobbyUpdate events
4. Updates CurrentLobbyID when lobby changes

**EventBus Subscription**:
```csharp
if (Di.Container.GetInstance<EventBus>() is EventBus eventBus)
{
    eventBus.Subscribe<LobbyUpdate>(e => GetCurrentId());
}
```

### Methods

#### GetCurrentId() (private)
Retrieves and updates current lobby ID from user info.

**Process**:
1. Gets user info from User.GetInstance()
2. Checks if LobbyId > 0
3. Sets CurrentLobbyID to LobbyId or DefaultIdMessage (0)

**Logic**:
```csharp
var userInfo = User.GetInstance().GetUserInfo();
CurrentLobbyID = userInfo.LobbyId > 0 ? userInfo.LobbyId : DefaultIdMessage;
```

### Event Handling

**LobbyUpdate Event**:
- Triggered when lobby state changes
- Automatically calls GetCurrentId()
- Updates UI via property binding

**Event Flow**:
```
Lobby created/joined/left
  ↓
LobbyService publishes LobbyUpdate
  ↓
EventBus notifies subscribers
  ↓
ViewModel.GetCurrentId()
  ↓
CurrentLobbyID updated
  ↓
UI refreshes
```

## LobbyService

**Purpose**: Manages lobby lifecycle and membership operations

### Dependencies
- `DatabaseService` - Database operations
- `EventBus` - Publishes LobbyUpdate events
- `ErrorService` - Error handling and user notifications
- `User` - Current user information

### Constants
- `EmptyLobbyId = 0` - No lobby assigned
- `DisconnectedLobbyId = -1` - User disconnected state

### Core Methods
#### StartApp()
Initializes application by loading current user's lobby and sending initial LobbyUpdate event.

**Flow**:
1. Get current user
2. Find user's lobby by ID
3. Publish LobbyUpdate event

#### CreateLobby()
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

#### ConnectToLobby(long lobbyId)
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

#### DisconnectFromLobby()
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

### Private Helper Methods

#### GetCurrentUserAsync()
Retrieves current user with null safety check.

#### FindLobbyAsync(long lobbyId)
Fetches lobby from database by ID with error handling.

#### SendLobbyEvent(Lobbies? lobbies)
Publishes LobbyUpdate event to EventBus with lobby member data.

#### DisconnectIfInLobby(Users userInfo)
Disconnects user from current lobby if already in one.

#### GenerateLobbyId()
Generates random unique lobby ID (1 to long.MaxValue).

#### CreateLobbyData(Users userInfo, long lobbyId)
Creates LobbyData entry for user-lobby association.

### Event System

#### LobbyUpdate Event
Published via EventBus when lobby state changes:
- User joins lobby
- User leaves lobby
- Lobby created
- Application starts

**Payload**: `List<LobbyData>` - Current lobby members

**Subscribers**: UI systems (avatar display panel, member lists, LobbyWindowViewModel)

### State Management
- `_isCreating` - Prevents concurrent lobby creation
- User lobby ID states:
  - `0` - No lobby
  - `-1` - Disconnected
  - `> 0` - Active lobby ID

### Usage Example
```csharp
// Create new lobby
await lobbyService.CreateLobby();

// Connect to friend's lobby
await lobbyService.ConnectToLobby(friendLobbyId);

// Leave lobby
await lobbyService.DisconnectFromLobby();
```

## UI Integration

### LobbyWindowViewModel Usage

**Initialization**:
```csharp
var viewModel = new LobbyWindowViewModel();
// CurrentLobbyID automatically populated
```

**XAML Binding**:
```xml
<TextBlock Text="{Binding CurrentLobbyID}"/>
<!-- Displays lobby ID or 0 if no lobby -->
```

**Automatic Updates**:
- ViewModel subscribes to LobbyUpdate events
- CurrentLobbyID updates when lobby changes
- UI refreshes automatically via binding

### Top Panel Integration
Top panel displays lobby members using avatars, updated automatically via LobbyUpdate event subscriptions.

---
