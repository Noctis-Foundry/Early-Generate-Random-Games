# MainWindow

## Overview
Primary application window managing navigation, lobby system integration, and user interface state.

## Files
- `MainWindow.axaml` - UI layout
- `MainWindow.axaml.cs` - Code-behind logic
- `MainWindowViewModel.cs` - ViewModel for data binding and commands

## Purpose
- Application entry point window
- Content navigation management
- Lobby system integration
- Database event handling
- User profile and avatar display
- Challenge rules access

## UI Structure

### Top Bar (DockPanel.Dock="Top")
- Menu items:
  - "Lobby System" - Opens lobby interface
  - "Rules" - Opens rules display
- User profile section:
  - Avatar image display
  - Lobby member avatars (dynamic)

### Content Area
- ContentControl for dynamic view switching
- Size: 1000x460

## Window Properties
- Title: "GameRandom"
- Size: 1000x500 (fixed)
- Background: Gray

## Key Components

### Injected Dependencies
```csharp
[Inject] private readonly LobbyService _lobby
[Inject] private readonly PostgresListener _postgres
[Inject] private readonly EventBus _eventBus
[Inject] private readonly UserControlFactory _controlFactory
[Inject] private readonly MainWindowFactory _mainWindowFactory
```

### Private Fields
```csharp
private readonly Register<string, Func<UserControl>> _preloadRegister
private readonly Action<string> _changeUserControlAction
private readonly Rules _rules
private readonly LobbyWindow _lobbyWindow
```

## Methods

### Constructor
Initializes window and all subsystems.

**Process**:
1. Initialize component
2. Skip if in design mode
3. Register services and resolve dependencies
4. Create LobbyWindow instance
5. Create and set MainWindowViewModel
6. Bind ViewModel commands
7. Initialize navigation action
8. Register user controls
9. Navigate to "Main" view
10. Initialize window events

### InitializeUserControlRegister()
Registers navigation targets.

**Registered Controls**:
- "Main" → MainWindowContent
- "Profile" → ProfileContent
- "Roll" → RollGame
- "Table" → GameTable

**Pattern**: Each control receives `_changeUserControlAction` for navigation

**Note**: TODO exists to change IUserControl to MainWindowUserControlAbstract for Profile and GameTable

### Navigate(string nameControl)
Switches active content view.

**Parameters**:
- `nameControl` - Registered control name

**Process**:
1. Retrieve control factory from register
2. Invoke factory to create control
3. Validate control creation
4. Cast to MainWindowUserControlAbstract
5. Set as ContentControl content
6. Call Open() on control

**Throws**: NullReferenceException if control creation fails

### MainWindow_OnClosed(object? sender, EventArgs e)
Cleanup on window close.

**Actions**:
1. Shutdown Steam integration
2. Exit application (code 0)

### InitWindowEvents()
Initializes window event subscriptions.

**Process**:
1. Subscribe to window Closing event
2. Call EventsConnecting()
3. Subscribe to EventBus LobbyUpdate events

### EventsConnecting()
Subscribes to database and lobby events.

**Process**:
1. Clear lobby images
2. Subscribe to PostgreSQL lobby table changes
3. Validate lobby service existence
4. Start lobby service asynchronously

**Database Events**: Listens to TableEnum.Lobby changes

**Throws**: Exception if lobby service is not found

### RegisterServiceWithMainWindowOwnerAndResolve(Window mainWindow)
Registers services with DI container and resolves dependencies.

**Parameters**:
- `mainWindow` - The main window instance

**Process**:
1. Register ErrorService singleton with window
2. Register ConfirmService singleton with window
3. Resolve injected fields for current instance

### UpdateLobby(int tableCode)
Updates lobby data and avatar grid on UI thread.

**Parameters**:
- `tableCode` - Database table code for validation

**Process**:
1. Validate DataContext is MainWindowViewModel
2. Invoke on UI thread asynchronously
3. Call ViewModel UpdateLobby method
4. Update avatar grid

### UpdateAvatarGrid()
Updates the avatar grid with lobby member images.

**Process**:
1. Validate DataContext is MainWindowViewModel
2. Get UsersToLobby from ViewModel
3. Clear existing lobby images and column definitions
4. Create cancellation token (5 seconds timeout)
5. For each profile:
   - Add new column definition
   - Create image in grid
   - Load avatar from Steam
   - Increment image counter

### BindingCommand()
Binds ViewModel commands to window actions.

**Bindings**:
- OpenLobbyCommand → Opens LobbyWindow
- RulesOpen → Opens Rules window

## Navigation System

### Registered Views
1. **Main** - Main menu/content
2. **Profile** - User profile display
3. **Roll** - Random game selection
4. **Table** - Game table view

### Navigation Flow
```
User Action → Navigate(name) → Factory → Create Control → Set Content → Open()
```

## Event Handling

### Lobby Updates
- Source: EventBus LobbyUpdate events
- Action: Update lobby data and avatar grid
- Thread: UI thread via Dispatcher

### Database Changes
- Source: PostgresListener
- Table: Lobby (TableEnum.Lobby)
- Action: Call UpdateLobby method
- Thread: UI thread via Dispatcher

### Window Closing
- Action: Shutdown Steam and exit application (code 0)

## Dependency Injection

### Resolved Services
- LobbyService - Lobby management
- PostgresListener - Database events
- EventBus - Application events
- UserControlFactory - Control creation
- MainWindowFactory - Factory for UI elements

### Registered Services
- ErrorService - Error handling (registered with window)
- ConfirmService - Confirmation dialogs (registered with window)

## ViewModel

### MainWindowViewModel
Manages lobby data and command bindings.

**Injected Dependencies**:
- SteamWebApi - Steam API integration
- DatabaseService - Database operations
- ErrorService - Error handling

**Properties**:
- `OpenLobbyCommand` - ICommand for opening lobby window
- `RulesOpen` - ICommand for opening rules window
- `UsersToLobby` - HashSet<ProfilerContext> of lobby members

**Private Fields**:
- `_isInitialized` - Initialization flag
- `_semaphore` - SemaphoreSlim(1,1) for thread-safe updates

**Methods**:

#### UpdateLobby(int tableCode)
Updates lobby data by loading information about all participants.

**Parameters**:
- `tableCode` - Must be TableEnum.Lobby

**Process**:
1. Acquire semaphore lock
2. Create 5-second cancellation token
3. Validate initialization state
4. Validate table code
5. Get current user data
6. Load lobby context from database
7. Validate lobby data
8. For each lobby member:
   - Fetch profile from Steam API
   - Add to UsersToLobby collection

**Error Handling**:
- Shows error if not initialized
- Shows error if table code incorrect
- Shows error if no lobby found
- Shows error if profile not found

#### BindingOpenLobbyCommand(Action func)
Binds the lobby opening command.

**Parameters**:
- `func` - Action to execute when opening lobby

**Behavior**: Creates RelayCommand if not already set

#### BindingRulesWindow(Action func)
Binds the rules window opening command.

**Parameters**:
- `func` - Action to execute when opening rules

**Behavior**: Creates RelayCommand if not already set

## UI Components

### Menu
- MenuItem: "Lobby System" → OpenLobbyCommand
- MenuItem: "Rules" → RulesOpen

### LobbyImages Grid
- Dynamic avatar display
- Updated on lobby changes
- Column-based layout with Star sizing
- Images loaded from Steam with 5-second timeout

### ControlMain
- ContentControl for view switching
- Fixed size: 1000x460

## Lifecycle

1. **Initialization**
   - DI resolution
   - Service registration
   - Control registration
   - Event subscription

2. **Runtime**
   - Navigation handling
   - Event processing
   - UI updates

3. **Shutdown**
   - Steam shutdown
   - Application exit

## Best Practices

1. Always use Navigate() for view switching
2. Register new views in InitializeUserControlRegister()
3. Use Dispatcher.UIThread for UI updates from events
4. Implement MainWindowUserControlAbstract for navigable views
5. Handle cleanup in MainWindow_OnClosed
6. Use cancellation tokens for async operations (5-second timeout)
7. Use semaphore for thread-safe lobby updates

## Integration Points

### Steam Integration
- Initialized before window
- Shutdown on close

### Database
- PostgresListener for real-time updates
- Lobby table monitoring

### Event System
- EventBus for application events
- LobbyUpdate event handling

### Dependency Injection
- Di.Container for field resolution
- Service registration on startup
- MainWindowFactory for UI element creation
