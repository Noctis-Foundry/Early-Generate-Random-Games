# IUserControl

## Overview
Interface defining contract for user control components with navigation support and lifecycle management.

## Purpose
- Standardize user control behavior
- Enable navigation callback injection
- Define open/close lifecycle methods
- Support factory pattern creation

## Methods

### AddListener(Action<string> onChangeContent)
Registers navigation callback for page transitions.

**Parameters**:
- `onChangeContent` - Callback invoked when control requests navigation

**Purpose**: Inject navigation logic from parent/factory

**Example**:
```csharp
control.AddListener(pageName => 
{
    NavigateToPage(pageName);
});
```

### Open()
Opens or displays the user control.

**Purpose**: Initialize and show control

**Typical Implementation**:
- Load data
- Subscribe to events
- Show UI elements
- Start animations

**Example**:
```csharp
public void Open()
{
    this.IsVisible = true;
    LoadData();
    SubscribeToEvents();
}
```

### Close(object? sender, RoutedEventArgs e)
Closes or hides the user control.

**Parameters**:
- `sender` - Event source
- `e` - Event arguments

**Purpose**: Cleanup and hide control

**Typical Implementation**:
- Unsubscribe from events
- Save state
- Hide UI elements
- Stop animations

**Example**:
```csharp
public void Close(object? sender, RoutedEventArgs e)
{
    UnsubscribeFromEvents();
    SaveState();
    this.IsVisible = false;
}
```

## Implementation Example

### Basic User Control
```csharp
public class MenuControl : UserControl, IUserControl
{
    private Action<string> _onNavigate;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _onNavigate = onChangeContent;
    }
    
    public void Open()
    {
        this.IsVisible = true;
        Logger.Info("Menu opened");
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        this.IsVisible = false;
        Logger.Info("Menu closed");
    }
    
    private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        _onNavigate?.Invoke("SettingsPage");
    }
}
```

### Advanced Implementation
```csharp
public class GameListControl : UserControl, IUserControl
{
    private Action<string> _onNavigate;
    private List<Game> _games;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _onNavigate = onChangeContent;
    }
    
    public void Open()
    {
        this.IsVisible = true;
        LoadGames();
        SubscribeToEvents();
        StartRefreshTimer();
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        UnsubscribeFromEvents();
        StopRefreshTimer();
        SaveScrollPosition();
        this.IsVisible = false;
    }
    
    private async void LoadGames()
    {
        _games = await gameService.GetGames();
        DisplayGames(_games);
    }
    
    private void OnGameSelected(Game game)
    {
        _onNavigate?.Invoke($"GameDetails/{game.Id}");
    }
}
```

## Usage with UserControlFactory

```csharp
var factory = new UserControlFactory();

var menuControl = factory.CreateUserControl<MenuControl>(pageName =>
{
    Console.WriteLine($"Navigating to: {pageName}");
    contentArea.Content = LoadPage(pageName);
});

menuControl.Open();
```

## Navigation Pattern

### Page Transitions
```csharp
public class MainWindow : Window
{
    private Dictionary<string, IUserControl> _pages = new();
    private IUserControl _currentPage;
    
    public MainWindow()
    {
        var factory = new UserControlFactory();
        
        _pages["Menu"] = factory.CreateUserControl<MenuControl>(NavigateToPage);
        _pages["Settings"] = factory.CreateUserControl<SettingsControl>(NavigateToPage);
        _pages["Game"] = factory.CreateUserControl<GameControl>(NavigateToPage);
    }
    
    private void NavigateToPage(string pageName)
    {
        _currentPage?.Close(null, null);
        
        if (_pages.TryGetValue(pageName, out var page))
        {
            _currentPage = page;
            _currentPage.Open();
            contentArea.Content = _currentPage;
        }
    }
}
```

## Lifecycle Flow

```
Creation → AddListener → Open → [Active] → Close
                          ↑                    ↓
                          └────────────────────┘
                          (Can reopen)
```

## Best Practices

1. **Null Check Navigation**: Always use `_onNavigate?.Invoke()`
2. **Cleanup in Close**: Unsubscribe events, stop timers
3. **Idempotent Open/Close**: Safe to call multiple times
4. **Store Navigation Callback**: Save in private field
5. **Use Visibility**: Toggle IsVisible instead of creating/destroying

## Common Patterns

### Modal Dialog Control
```csharp
public class ConfirmDialog : UserControl, IUserControl
{
    private Action<string> _onNavigate;
    private TaskCompletionSource<bool> _result;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _onNavigate = onChangeContent;
    }
    
    public void Open()
    {
        this.IsVisible = true;
        _result = new TaskCompletionSource<bool>();
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        this.IsVisible = false;
        _result?.TrySetResult(false);
    }
    
    public Task<bool> ShowAsync()
    {
        Open();
        return _result.Task;
    }
}
```

### Animated Control
```csharp
public class SplashControl : UserControl, IUserControl
{
    private Action<string> _onNavigate;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _onNavigate = onChangeContent;
    }
    
    public async void Open()
    {
        this.IsVisible = true;
        await PlayOpenAnimation();
        
        // Auto-navigate after splash
        await Task.Delay(3000);
        _onNavigate?.Invoke("MainMenu");
    }
    
    public async void Close(object? sender, RoutedEventArgs e)
    {
        await PlayCloseAnimation();
        this.IsVisible = false;
    }
}
```

## Integration with MVVM

```csharp
public class GameViewModel : ViewModelBase
{
    private Action<string> _navigate;
    
    public void SetNavigationCallback(Action<string> navigate)
    {
        _navigate = navigate;
    }
    
    public void NavigateToSettings()
    {
        _navigate?.Invoke("Settings");
    }
}

public class GameControl : UserControl, IUserControl
{
    private GameViewModel _viewModel;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _viewModel.SetNavigationCallback(onChangeContent);
    }
    
    public void Open()
    {
        this.IsVisible = true;
        _viewModel.LoadData();
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        this.IsVisible = false;
    }
}
```

## Features

- **Standardized Interface**: Consistent control behavior
- **Navigation Support**: Built-in page transition mechanism
- **Lifecycle Management**: Clear open/close semantics
- **Factory Compatible**: Works with UserControlFactory
- **Flexible Implementation**: Minimal constraints on implementation

## Limitations

- No async Open/Close support
- Single navigation callback (no multi-cast)
- No built-in state management
- Close requires event arguments (even if unused)
