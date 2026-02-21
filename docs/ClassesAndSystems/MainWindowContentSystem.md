# Main Window Content System

## Overview
Main navigation hub with three large image-based buttons for navigating to different sections of the application. Features animated gradient borders on hover.

## Purpose
- Primary navigation interface
- Visual menu with image buttons
- Animated hover effects
- Page routing via callbacks

## Components

### MainWindowContent.axaml (View)
UserControl with 2x2 grid layout containing three navigation buttons.

**Layout**: 
- Max size: 1920x1080
- Design size: 1000x600
- 2 rows × 2 columns (equal star sizing)
- Background: #778899 (LightSlateGray)

**Navigation Buttons**:

**Table Button** (Top-Left)
- Position: Grid.Column="0" Grid.Row="0"
- Alignment: Left, Top
- Image: `LeftUpClick.png`
- Action: Navigate to "Table" page

**Profile Button** (Bottom-Left)
- Position: Grid.Column="0" Grid.Row="1"
- Alignment: Left, Bottom
- Image: `LeftDownClick.png`
- Action: Navigate to "Profile" page

**Roll Button** (Right, Full Height)
- Position: Grid.Column="1" Grid.Row="0" Grid.RowSpan="2"
- Alignment: Left, Top
- Image: `RightClick.png`
- Action: Navigate to "Roll" page
- Spans both rows

### MainWindowContent.axaml.cs (Code-behind)
Implements IUserControl with navigation callback handling.

**Fields**:
- `_changeContent` (Action<string>) - Navigation callback

**Methods**:

**AddListener(Action<string> onChangeContent)**
Registers navigation callback from parent.

**Open()**
Empty implementation (no initialization needed).

**Close(object? sender, RoutedEventArgs e)**
Empty implementation (no cleanup needed).

**GoToRollContent(object? sender, RoutedEventArgs e)**
Navigates to "Roll" page.

**GoToTable(object? sender, RoutedEventArgs e)**
Navigates to "Table" page.

**GoToProfile(object? sender, RoutedEventArgs e)**
Navigates to "Profile" page.

**GoToRules(object? sender, RoutedEventArgs e)**
Navigates to "Rules" page (not connected to any button).

### MainWindowStyle.axaml (Styles)
Defines button styling with animated hover effects.

**Style Classes**:

**MainWindowButtons** (Button)
- Padding: 0
- Background: Transparent

**MainWindowButtons:pointerover** (Hover State)
- Border thickness: 2px
- Animated rotating conic gradient border
- Duration: 2 seconds
- Infinite loop

**Gradient Colors**:
- Magenta (#FF00FF) at 0%
- Medium Turquoise (#48D1CC) at 50%
- Slate Blue (#6A5ACD) at 100%

**Animation**: Rotates gradient from 0° to 360°

## Visual Layout

```
┌─────────────┬─────────────┐
│   Table     │             │
│   Button    │    Roll     │
│             │   Button    │
├─────────────┤  (Full      │
│  Profile    │   Height)   │
│   Button    │             │
└─────────────┴─────────────┘
```

## Navigation Flow

```
MainWindowContent
    ├─> "Table" → Table page
    ├─> "Profile" → Profile page
    └─> "Roll" → Roll/Game selection page
```

## Usage Example

### Creating and Initializing
```csharp
var factory = new UserControlFactory();

var mainContent = factory.CreateUserControl<MainWindowContent>(pageName =>
{
    switch (pageName)
    {
        case "Table":
            contentArea.Content = new TableView();
            break;
        case "Profile":
            contentArea.Content = new ProfileView();
            break;
        case "Roll":
            contentArea.Content = new RollView();
            break;
    }
});

mainContent.Open();
contentArea.Content = mainContent;
```

### Integration with Main Window
```csharp
public class MainWindow : Window
{
    private Dictionary<string, UserControl> _pages = new();
    private UserControl _currentPage;
    
    public MainWindow()
    {
        InitializeComponent();
        
        var factory = new UserControlFactory();
        var mainContent = factory.CreateUserControl<MainWindowContent>(NavigateToPage);
        
        _pages["Main"] = mainContent;
        _pages["Table"] = new TableView();
        _pages["Profile"] = new ProfileView();
        _pages["Roll"] = new RollView();
        
        NavigateToPage("Main");
    }
    
    private void NavigateToPage(string pageName)
    {
        if (_pages.TryGetValue(pageName, out var page))
        {
            _currentPage = page;
            contentArea.Content = page;
        }
    }
}
```

## Image Assets

Required images in `Assets/` folder:

**LeftUpClick.png**
- Table/Games list button image
- Position: Top-left quadrant

**LeftDownClick.png**
- Profile button image
- Position: Bottom-left quadrant

**RightClick.png**
- Roll/Random game button image
- Position: Right half (full height)

## Hover Animation Details

### Conic Gradient Rotation
- **Start**: 0° angle
- **End**: 360° angle
- **Duration**: 2 seconds
- **Iteration**: Infinite

### Color Stops
1. **Magenta** (#FF00FF) - Vibrant pink/purple
2. **Medium Turquoise** (#48D1CC) - Cyan/aqua
3. **Slate Blue** (#6A5ACD) - Purple/blue

### Visual Effect
Creates a rotating rainbow-like border that continuously spins around the button when hovered.

## Features

- **Large Touch Targets**: Full quadrant buttons for easy clicking
- **Visual Feedback**: Animated borders on hover
- **Image-Based Navigation**: Visual menu instead of text
- **Responsive Layout**: Star-sized grid adapts to window size
- **IUserControl Compatible**: Works with UserControlFactory
- **Callback-Based Navigation**: Decoupled from parent implementation

## Best Practices

1. **Provide Clear Images**: Use descriptive, high-contrast images
2. **Optimize Image Size**: Match image resolution to display size
3. **Test Hover States**: Verify animation performance
4. **Handle Navigation Errors**: Check if target page exists
5. **Preload Pages**: Create pages during initialization

## Limitations

- GoToRules method exists but no button connected
- Empty Open/Close implementations
- No loading states during navigation
- No back button functionality
- Images must exist or buttons show broken image
- No keyboard navigation support
- Fixed grid layout (not responsive to small screens)

## Potential Improvements

```csharp
public partial class MainWindowContent : UserControl, IUserControl
{
    private Action<string> _changeContent;
    private bool _isNavigating;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _changeContent = onChangeContent;
    }
    
    public void Open()
    {
        this.IsVisible = true;
        this.IsEnabled = true;
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        this.IsVisible = false;
    }
    
    private async void NavigateTo(string pageName)
    {
        if (_isNavigating) return;
        
        _isNavigating = true;
        
        try
        {
            // Show loading indicator
            ShowLoadingOverlay();
            
            // Invoke navigation
            _changeContent?.Invoke(pageName);
            
            // Log navigation
            Logger.Info($"Navigated to {pageName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Navigation failed: {ex.Message}");
        }
        finally
        {
            HideLoadingOverlay();
            _isNavigating = false;
        }
    }
    
    private void GoToRollContent(object? sender, RoutedEventArgs e) => NavigateTo("Roll");
    private void GoToTable(object? sender, RoutedEventArgs e) => NavigateTo("Table");
    private void GoToProfile(object? sender, RoutedEventArgs e) => NavigateTo("Profile");
    private void GoToRules(object? sender, RoutedEventArgs e) => NavigateTo("Rules");
}
```

### Enhanced XAML with Tooltips
```xml
<Button Grid.Column="0" Grid.Row="0" 
        Classes="MainWindowButtons" 
        Click="GoToTable"
        ToolTip.Tip="View game table">
    <Image Source="avares://GameRandom/Assets/LeftUpClick.png" 
           Stretch="UniformToFill"/>
</Button>

<Button Grid.Column="0" Grid.Row="1" 
        Classes="MainWindowButtons" 
        Click="GoToProfile"
        ToolTip.Tip="View your profile">
    <Image Source="avares://GameRandom/Assets/LeftDownClick.png" 
           Stretch="UniformToFill"/>
</Button>

<Button Grid.Column="1" Grid.Row="0" Grid.RowSpan="2" 
        Classes="MainWindowButtons" 
        Click="GoToRollContent"
        ToolTip.Tip="Roll for random game">
    <Image Source="avares://GameRandom/Assets/RightClick.png" 
           Stretch="UniformToFill"/>
</Button>
```

### Keyboard Navigation Support
```xml
<Button Grid.Column="0" Grid.Row="0" 
        Classes="MainWindowButtons" 
        Click="GoToTable"
        HotKey="Ctrl+T">
    <Image Source="avares://GameRandom/Assets/LeftUpClick.png"/>
</Button>
```

## Integration with EventBus

```csharp
public partial class MainWindowContent : UserControl, IUserControl
{
    [Inject] private readonly EventBus _eventBus = null!;
    
    private void GoToRollContent(object? sender, RoutedEventArgs e)
    {
        _eventBus.Publish(new NavigationRequested("Roll"));
        _changeContent?.Invoke("Roll");
    }
}

public record NavigationRequested(string PageName);
```

## Testing

```csharp
[Test]
public void TestNavigation()
{
    var mainContent = new MainWindowContent();
    string navigatedTo = null;
    
    mainContent.AddListener(page => navigatedTo = page);
    
    // Simulate button click
    mainContent.GoToTable(null, null);
    
    Assert.AreEqual("Table", navigatedTo);
}
```
