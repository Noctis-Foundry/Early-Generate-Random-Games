# Current Game Status System

## Overview
UI system for displaying current game session information including game name, dates, time spent, and action buttons. Uses MVVM pattern with Avalonia UI.

## Purpose
- Display active game session details
- Show time tracking information
- Provide quick access to Steam page
- Check game running status

## Components

### CurrentGame.axaml (View)
Modal window displaying game session information.

**Layout**: 400x300 window with 2-column grid

**Left Column** (Game Info):
- Game name
- Start date
- Current date
- Time spent
- End date

**Right Column** (Visual & Actions):
- Game cover image (140px width, spans 4 rows)
- Action buttons (Steam, Check status)

**Design**:
- Gray background container
- Dark info cards (#444444)
- White text, centered
- Animated rotating gradient border on image

### CurrentGameStatusViewModel.cs (ViewModel)
ViewModel managing game status data and business logic.

**Properties**:
- `database` (DatabaseService) - Injected database service

**Methods**:
- `LoadInfo()` - Loads game information (currently empty)

**Data Model**:
- `GameStatusInfo` - Placeholder for game status data

### CurrentGameStatusStyle.axaml (Styles)
XAML styles defining visual appearance.

**Style Classes**:

**GameInfoContainer** (DockPanel)
- Background: Gray
- Margin: 5px

**GameInfoGrid** (Grid)
- Alignment: Center
- Row/Column spacing: 8px

**GameInfoItem** (Border)
- Background: #444444
- Corner radius: 4px
- Padding: 8px, 5px

**GameInfoText** (TextBlock)
- Foreground: White
- Font size: 14px
- Centered alignment
- Text wrapping enabled
- Max width: 200px
- Max height: 80px

**GameImageBorder** (Border)
- Border thickness: 1px
- Corner radius: 4px
- Animated rotating conic gradient (3s loop)
- Colors: White → Silver → Gray

**GameInfoButton** (Button)
- Background: #555555
- Foreground: White
- Font size: 14px
- Padding: 8px, 5px
- Corner radius: 4px
- Hover: Animated gradient border

### CurrentGame.axaml.cs (Code-behind)
Minimal code-behind with initialization only.

## UI Elements

### Information Display

**GameName** (TextBlock)
- Example: "Name: Dead cells"
- Displays current game title

**StartDate** (TextBlock)
- Example: "Start: 24.04.2026"
- Shows session start date

**TodayDate** (TextBlock)
- Example: "Today: 26.04.2026"
- Current date reference

**TimeSpent** (TextBlock)
- Example: "Time: 30day 14h 52m"
- Total time played

**EndDate** (TextBlock)
- Example: "End date: 24.05.2026"
- Expected completion date

### Action Buttons

**Steam Button**
- Opens game's Steam store page
- TODO: Implementation pending

**Check Status Button**
- Verifies if game is currently running
- TODO: Implementation pending

## Visual Features

### Animated Border
Rotating conic gradient animation on game image:
- Duration: 3 seconds
- Infinite loop
- Gradient: White → Silver → Gray
- Rotation: 0° to 360°

### Hover Effects
Button hover state shows animated gradient border matching image animation.

## Data Flow

```
Database → ViewModel.LoadInfo() → View Properties → UI Display
```

## Usage Example

### Opening Window
```csharp
var gameWindow = new CurrentGame();
gameWindow.DataContext = new CurrentGameStatusViewModel();
await gameWindow.ShowDialog(mainWindow);
```

### Loading Game Data
```csharp
public class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private DatabaseService database = null!;
    
    public string GameName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TimeSpent { get; set; }
    
    public async void LoadInfo()
    {
        var userGame = await database.GetUserGameByAppId(currentAppId);
        
        if (userGame != null)
        {
            GameName = userGame.AppName;
            StartDate = userGame.BeginData ?? DateTime.Now;
            EndDate = userGame.EndData ?? DateTime.Now;
            TimeSpent = EndDate - StartDate;
        }
    }
}
```

### Binding in XAML
```xml
<TextBlock Text="{Binding GameName}" Classes="GameInfoText"/>
<TextBlock Text="{Binding StartDate, StringFormat='Start: {0:dd.MM.yyyy}'}" Classes="GameInfoText"/>
<TextBlock Text="{Binding TimeSpent, StringFormat='Time: {0:dd}day {0:hh}h {0:mm}m'}" Classes="GameInfoText"/>
```

## Integration Points

### Database Service
Retrieves game session data via injected DatabaseService.

### Steam Integration
- Steam button opens game store page
- Check status verifies game running state

### Time Tracking
Calculates and displays time spent based on start/end dates.

## Current State

### Implemented
- UI layout and styling
- Animated visual effects
- Basic window structure
- ViewModel skeleton

### TODO
- LoadInfo() implementation
- GameStatusInfo data model
- Steam button functionality
- Check status button logic
- Data binding setup
- Time calculation logic

## Potential Enhancements

### ViewModel Implementation
```csharp
public class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private DatabaseService database = null!;
    
    private string _gameName;
    public string GameName
    {
        get => _gameName;
        set => SetProperty(ref _gameName, value);
    }
    
    private string _startDate;
    public string StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }
    
    private string _timeSpent;
    public string TimeSpent
    {
        get => _timeSpent;
        set => SetProperty(ref _timeSpent, value);
    }
    
    public async void LoadInfo(int appId)
    {
        var game = await database.GetUserGameByAppId(appId);
        
        if (game != null)
        {
            GameName = $"Name: {game.AppName}";
            StartDate = $"Start: {game.BeginData:dd.MM.yyyy}";
            
            var timeSpan = DateTime.Now - (game.BeginData ?? DateTime.Now);
            TimeSpent = $"Time: {timeSpan.Days}day {timeSpan.Hours}h {timeSpan.Minutes}m";
        }
    }
    
    public void OpenSteamPage()
    {
        // Open Steam store page
        Process.Start(new ProcessStartInfo
        {
            FileName = $"steam://store/{appId}",
            UseShellExecute = true
        });
    }
    
    public async Task<bool> CheckGameStatus()
    {
        // Check if game is running via Steam API
        return SteamApps.BIsAppInstalled(new AppId_t((uint)appId));
    }
}
```

### GameStatusInfo Model
```csharp
public class GameStatusInfo
{
    public int AppId { get; set; }
    public string GameName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public bool IsRunning { get; set; }
    public string ImageUrl { get; set; }
}
```

## Features

- **Clean UI**: Dark theme with clear information hierarchy
- **Visual Appeal**: Animated gradient borders
- **Responsive Layout**: Grid-based flexible layout
- **MVVM Pattern**: Separation of concerns
- **DI Integration**: Injected database service
- **Extensible**: Easy to add new info fields

## Limitations

- Hardcoded example data in XAML
- No data binding implemented
- Button actions not implemented
- No error handling
- Static image source
- No real-time updates
