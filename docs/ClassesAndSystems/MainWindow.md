# MainWindow System

## Overview
Primary application window managing navigation, lobby integration, and global UI state. Serves as the root container for all user controls and handles Steam/database initialization.

## Purpose
- Host all user controls via ContentControl
- Display top menu bar with lobby avatars
- Manage navigation between pages
- Initialize services and event subscriptions
- Handle application lifecycle

## Layout

### Window Properties
- Size: 1000x500 (fixed)
- Min/Max: 1000x500 (non-resizable)
- Background: Black
- Icon: avalonia-logo.ico
- Title: "GameRandom"

### Structure
- **Row 0**: Top menu bar (Auto height)
- **Row 1**: Content area (fills remaining space)

### Top Menu Bar

**Left Section** (Menu):
- Width: 600px, Height: 30px
- Background: MediumAquamarine
- Menu items:
  - "Lobby" - Opens lobby window
  - "Show Error" - Test error display
  - "Rules" - Opens create lobby window

**Right Section** (LobbyImages Grid):
- Width: 363px, Height: 30px
- Background: MediumAquamarine
- 2 columns (40px each)
- Displays lobby member avatars
- Dynamically updated via EventBus

### Content Area
- ContentControl named "ControlMain"
- Hosts current page (Main, Profile, Roll, Table)

## Code-behind (MainWindow.axaml.cs)

### Injected Dependencies
- `_lobby` (LobbyService) - Lobby management
- `_diFactory` (DiFactory) - Dependency injection factory
- `_postgres` (PostgresListener) - Database change notifications
- `_eventBus` (EventBus) - Event pub/sub system
- `_controlFactory` (UserControlFactory) - User control creation

### Fields
- `_preloadRegister` (Register<string, IUserControl>) - Preloaded pages
- `_selectorAction` (Action<string>) - Navigation callback
- `_oldControl` (IUserControl?) - Previous control reference

### Constructor

**Process**:
1. Initialize component
2. Skip if design mode
3. Resolve DI dependencies
4. Register ErrorService with window
5. Create MainWindowViewModel
6. Set DataContext
7. Initialize navigation action
8. Preload user controls
9. Navigate to "Main" page
10. Subscribe to Closing event
11. Connect database events
12. Subscribe to LobbyUpdate events

### Methods

**InitializeUserControlRegister()**
Preloads and registers all user controls.

**Registered Pages**:
- "Main" → MainWindowContent
- "Profile" → ProfileContent
- "Roll" → RollGame
- "Table" → GameTable (commented out)

**Navigate(string nameControl)**
Switches displayed content to specified page.

**Process**:
1. Lookup control in register
2. Set as ContentControl content
3. Call Open() on control

**MainWindow_OnClosed(object? sender, EventArgs e)**
Cleanup on window close.

**Actions**:
- Shuts down Steam API

**EventsConnecting()**
Sets up event subscriptions and initializations.

**Process**:
1. Clear lobby images grid
2. Subscribe to PostgresListener for lobby changes
3. Validate lobby service
4. Start lobby app (loads current user's lobby)

**RegisterUiService(Window window)**
Registers ErrorService with main window for modal dialogs.

**Validation**: Ensures window is MainWindow type

## ViewModel Integration

### MainWindowViewModel
Handles menu commands and lobby avatar updates.

**Commands**:
- OpenLobby - Opens lobby management window
- ShowError - Test error display
- OpenCreateLobbyWindow - Opens lobby creation

**Methods**:
- UpdateLobby(Grid grid, int tableCode) - Updates lobby avatars

## Event Flow

### LobbyUpdate Event
```
Database change → PostgresListener → EventBus.Publish(LobbyUpdate) →
MainWindow subscription → ViewModel.UpdateLobby() → UI update
```

### Navigation Flow
```
User action → Navigate("PageName") → 
Lookup in register → Set ContentControl.Content → 
Call IUserControl.Open()
```

## Preloading Strategy

All main pages preloaded at startup:
- Faster navigation (no instantiation delay)
- Maintains state between navigations
- Uses Register<string, IUserControl> for storage

## Lifecycle

### Startup
1. Window constructor
2. DI resolution
3. Service registration
4. ViewModel creation
5. Control preloading
6. Event subscriptions
7. Navigate to Main
8. Lobby initialization

### Navigation
1. User triggers navigation
2. Navigate() called with page name
3. Old control remains in memory
4. New control displayed
5. Open() called on new control

### Shutdown
1. Closing event fired
2. Steam API shutdown
3. Window closes

## Features

- **Preloaded Pages**: Instant navigation
- **Event-Driven**: Real-time lobby updates
- **DI Integration**: All services injected
- **Steam Integration**: Automatic initialization/cleanup
- **Database Monitoring**: PostgresListener subscriptions
- **Fixed Size**: Consistent window dimensions

## Integration Example

```csharp
// App.axaml.cs
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var mainWindow = new MainWindow();
        desktop.MainWindow = mainWindow;
    }
    
    base.OnFrameworkInitializationCompleted();
}
```

## Limitations

- Fixed window size (non-resizable)
- Table page commented out
- No back button navigation
- Preloads all pages (memory overhead)
- No lazy loading
- Hardcoded page names

## Potential Improvements

```csharp
public partial class MainWindow : Window
{
    private Stack<string> _navigationHistory = new();
    
    public void Navigate(string nameControl, bool addToHistory = true)
    {
        if (_preloadRegister.GetObjectFromRegister(nameControl, out var value))
        {
            if (value is null) return;
            
            _oldControl?.Close(null, null);
            
            ControlMain.Content = value;
            value.Open();
            
            if (addToHistory)
                _navigationHistory.Push(nameControl);
            
            _oldControl = value;
        }
    }
    
    public void NavigateBack()
    {
        if (_navigationHistory.Count > 1)
        {
            _navigationHistory.Pop(); // Remove current
            var previous = _navigationHistory.Pop(); // Get previous
            Navigate(previous, addToHistory: false);
        }
    }
    
    // Lazy loading alternative
    private IUserControl GetOrCreateControl(string name)
    {
        if (!_preloadRegister.GetObjectFromRegister(name, out var control))
        {
            control = CreateControl(name);
            _preloadRegister.RegisterNewObject(name, control);
        }
        return control;
    }
}
```

## Testing

```csharp
[Test]
public void TestNavigation()
{
    var mainWindow = new MainWindow();
    
    mainWindow.Navigate("Profile");
    Assert.IsInstanceOf<ProfileContent>(mainWindow.ControlMain.Content);
    
    mainWindow.Navigate("Main");
    Assert.IsInstanceOf<MainWindowContent>(mainWindow.ControlMain.Content);
}
```
