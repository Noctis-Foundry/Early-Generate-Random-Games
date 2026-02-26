# MainWindow

## Overview
Primary application window managing navigation, lobby system integration, and user interface state.

## Files
- `MainWindow.axaml` - UI layout
- `MainWindow.axaml.cs` - Code-behind logic

## Purpose
- Application entry point window
- Content navigation management
- Lobby system integration
- Database event handling
- User profile display

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
[Inject] private readonly DiFactory _diFactory
[Inject] private readonly PostgresListener _postgres
[Inject] private readonly EventBus _eventBus
[Inject] private readonly UserControlFactory _controlFactory
```

### Private Fields
```csharp
private readonly Register<string, Func<UserControl>> _preloadRegister
private readonly Action<string> _selectorAction
private IUserControl? _oldControl
```

## Methods

### Constructor
Initializes window and all subsystems.

**Process**:
1. Initialize component
2. Skip if in design mode
3. Resolve dependencies via DI
4. Register UI service
5. Create and set ViewModel
6. Initialize navigation action
7. Register user controls
8. Navigate to "Main" view
9. Subscribe to closing event
10. Connect database events
11. Subscribe to lobby updates

### InitializeUserControlRegister()
Registers navigation targets.

**Registered Controls**:
- "Main" → MainWindowContent
- "Profile" → ProfileContent
- "Roll" → RollGame
- "Table" → GameTable

**Pattern**: Each control receives `_selectorAction` for navigation

### Navigate(string nameControl)
Switches active content view.

**Parameters**:
- `nameControl` - Registered control name

**Process**:
1. Retrieve control factory from register
2. Invoke factory to create control
3. Validate control creation
4. Cast to IUserControl
5. Set as ContentControl content
6. Call Open() on control

**Throws**: NullReferenceException if control creation fails

### MainWindow_OnClosed(object? sender, EventArgs e)
Cleanup on window close.

**Actions**:
1. Shutdown Steam integration
2. Exit application (code 0)

### EventsConnecting()
Initializes event subscriptions.

**Process**:
1. Clear lobby images
2. Subscribe to PostgreSQL lobby table changes
3. Update lobby UI on changes
4. Start lobby service

**Database Events**: Listens to TableEnum.Lobby changes

### RegisterUiService(Window window)
Registers error service with DI container.

**Validation**: Ensures window is MainWindow type

**Throws**: Exception if window type mismatch

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
- Action: Update lobby images in UI thread
- Target: LobbyImages grid

### Database Changes
- Source: PostgresListener
- Table: Lobby (TableEnum.Lobby)
- Action: Update lobby UI asynchronously
- Thread: UI thread via Dispatcher

### Window Closing
- Action: Shutdown Steam and exit application

## Dependency Injection

### Resolved Services
- LobbyService - Lobby management
- DiFactory - DI factory
- PostgresListener - Database events
- EventBus - Application events
- UserControlFactory - Control creation

### Registered Services
- ErrorService - Error handling (registered with window)

## ViewModel

### MainWindowViewModel
Created with WindowService wrapper.

**Responsibilities**:
- Command handling (OpenLobbyCommand, RulesOpen)
- Lobby UI updates
- Window service integration

## UI Components

### Menu
- MenuItem: "Lobby System" → OpenLobbyCommand
- MenuItem: "Rules" → RulesOpen

### LobbyImages Grid
- Dynamic avatar display
- Updated on lobby changes
- Column-based layout

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
4. Implement IUserControl for navigable views
5. Handle cleanup in MainWindow_OnClosed

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
- DiFactory for service creation
