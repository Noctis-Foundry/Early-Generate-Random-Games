# Profile System

## Overview
Comprehensive user profile system displaying player statistics, game history, and Steam avatar. Features animated UI elements, statistics cards, and a detailed games table window.

## Purpose
- Display user profile with Steam avatar and nickname
- Show player statistics (games count, finished games)
- List all played games in a data grid
- Provide navigation to activity tracking
- Animated visual effects throughout

## Components

### ProfileContent.axaml (Main View)
UserControl with profile layout and statistics display.

**Layout**:
- Size: 1000x500 (min), 1920x1080 (max)
- Background: AppBackground.png image
- Top bar with title and action buttons
- Left: Statistics panel (700x400)
- Right: Avatar card (230x280)

**UI Elements**:

**Top Bar**:
- Title: "Profile" with letter spacing
- "Activity game" button
- "Close" button (navigates to Main)

**Avatar Card**:
- Steam avatar (140x140, rounded corners)
- Player nickname below avatar
- Animated rotating gradient border
- Background: AvatarBorder.png

**Statistics Area**:
- ContentControl hosting StatisticControl
- Dynamically loaded on Open()

### ProfileContent.axaml.cs (Code-behind)
Implements IUserControl and IDisposable for lifecycle management.

**Methods**:

**Open()**
- Loads Steam avatar via SteamFriends API
- Gets player nickname
- Creates and opens StatisticControl
- Sets content to statistics panel

**Close(object? sender, RoutedEventArgs e)**
- Navigates back to "Main" page
- Calls Dispose() for cleanup

**InitProfileAvatar()**
- Retrieves Steam ID
- Gets persona name from Steam
- Loads large friend avatar
- Converts to Avalonia bitmap
- Updates UI elements

**Dispose()**
- Clears navigation callback
- Releases image source
- Nulls DataContext

### StatisticControl.axaml (Statistics View)
UserControl displaying player statistics in card layout.

**Layout**:
- Size: 700x400
- Background: TableBorder1920.png
- Header with title and "Games Table" button
- Grid of statistic cards (2 rows × 3 columns)

**Header**:
- "Player Statistic" title
- Animated rotating gradient border (3s loop)
- "Games Table" button to open detailed view

**Card Grid**:
- 15px spacing between cards
- Dynamically populated cards
- Each card: 130x100, rounded corners

### StatisticControl.axaml.cs (Code-behind)
Manages statistics loading and table window.

**Methods**:

**Open()**
- Calls LoadStatisticAsync on ViewModel
- Populates StatisticCardGrid

**Close()**
- Disposes ViewModel

**OpenTable(object? sender, RoutedEventArgs e)**
- Creates GamesTableWindow
- Opens table window

### GamesTableWindow.axaml (Games Table Window)
Separate window displaying all played games in DataGrid.

**Layout**:
- Size: 800x500 (default), 600x500 (min), 1000x700 (max)
- Gray background
- DataGrid with ProfileDataGrid style

**Columns**:
- Game name
- Begin date
- End date

**Data Binding**: Bound to GameProgresses collection

### GamesTableWindow.axaml.cs (Code-behind)
Window lifecycle management.

**Methods**:

**Open()**
- Shows window
- Calls LoadTable() on ViewModel

**OnClosed(EventArgs e)**
- Calls UnloadTable() on ViewModel
- Cleanup resources

## ViewModels

### ProfileViewModel
Empty ViewModel placeholder for future profile-specific logic.

### StatisticViewModel
Manages statistics card generation and data loading.

**Fields**:
- `_dbService` (DatabaseService) - Injected database service

**Methods**:

**LoadStatisticAsync(Grid grid)**
- Resolves DI dependencies
- Queries GameProgresses for current user
- Calculates statistics (total games, finished games)
- Dynamically creates statistic cards
- Adds cards to grid

**FactoryNewCard(Grid grid, StatisticCardInfo cardInfo)**
- Creates Border with StatisticPropertyBorder style
- Creates Grid with 3 rows (title, separator, data)
- Adds TextBlocks with StatisticCardText style
- Sets grid position
- Adds to parent grid

**Dispose()**
- Nulls database service reference

**StatisticCardInfo Class**:
- Title (string) - Card label
- Data (string) - Card value
- Row (int) - Grid row position
- Column (int) - Grid column position

### StatisticGameTableViewModel
Manages games table data loading and conversion.

**Fields**:
- `_database` (DatabaseService)
- `_errorService` (ErrorService)
- `_converter` (ObservableConverter)
- `_gameProgresses` (ObservableCollection<ProfileTableData>)

**Properties**:
- GameProgresses - Observable collection for DataGrid binding

**Methods**:

**LoadTable()**
- Resolves DI dependencies
- Gets current Steam user ID
- Loads all GameProgresses from database
- Filters by user ID
- Converts to ProfileTableData
- Updates observable collection on UI thread

**ToPlayerTableData(List<GameProgresses> gameProgress, ulong userId)**
- Filters games by player ID
- Converts to ProfileTableData format
- Formats dates as "yy-MM-dd"
- Returns list or null if empty

**UnloadTable()**
- Clears observable collection
- Nulls all references
- Cleanup on window close

**ProfileTableData Class**:
- GameName (string)
- DataBegin (string) - Formatted start date
- DataEnd (string) - Formatted end date

## Styles

### ProfileStyle.axaml

**TopProfileContainer** (Border):
- Corner radius: 20px
- Border: 2px linear gradient (purple to slate gray)
- Transparent background

**TitleTextBoxStyle** (TextBlock):
- Font size: 20px
- Foreground: #F2F2F2 (light gray)
- Letter spacing: 5px
- Left margin: 20px

**ProfileButtons** (Button):
- Font size: 16px
- Corner radius: 30px
- Transparent background
- Border: 2px gradient (purple shades)
- Foreground: #EDEDED

**ProfileButtons:pointerover**:
- Foreground: #366359 (dark teal)
- Background: LightGray
- Animated rotating conic gradient border (2s loop)

**AvatarBorder** (Border):
- Corner radius: 30px
- Border: 3px animated rotating gradient (1s loop)
- Colors: Light blue to slate gray

**ProfileNickname** (TextBlock):
- Font size: 24px
- Foreground: #EDEDED
- Condensed font stretch

**ProfileAvatar** (Border):
- Size: 150-160px (width), 150-190px (height)
- Border: 2px gradient (magenta to purple)

### StatisticStyle.axaml

**CardContainer** (Grid):
- Column spacing: 15px
- Row spacing: 15px
- Margin: 15px top/left

**StatisticPropertyBorder** (Border):
- Background: CadetBlue
- Size: 130x100
- Corner radius: 10px

**CardBorderGrid** (Grid):
- Vertical alignment: Center
- Row spacing: 10px

**StatisticCardText** (TextBlock):
- Font: Anime Ace
- Foreground: White
- Font size: 14px
- Centered alignment
- Text wrapping enabled

**StatisticContainer** (Border):
- Transparent background
- Top padding: 4px

**StatisticHeader** (Border):
- Border: 2px animated rotating gradient (3s loop)
- Corner radius: 20px
- Padding: 20px horizontal
- Colors: Light blue to slate gray

**StatisticHeaderText** (TextBlock):
- Foreground: #D8D8D8
- Letter spacing: 5px
- Font size: 18px

### ProfileDataGridStyle.axaml

**ProfileDataGrid** (DataGrid):
- Border: 2px AntiqueWhite
- Background: #2f2f2d (dark gray)
- Grid lines: All visible
- Resizable columns

**DataGridColumnHeader**:
- Background: DimGray
- Text: WhiteSmoke, size 15px
- Font: Anime Ace

**DataGridRow**:
- Background: SlateGray
- Text: White

## Data Flow

```
Steam API → ProfileContent.InitProfileAvatar() → UI (Avatar + Nickname)
                                                    ↓
Database → StatisticViewModel.LoadStatisticAsync() → Dynamic Cards
                                                    ↓
Database → StatisticGameTableViewModel.LoadTable() → DataGrid
```

## Usage Example

### Opening Profile
```csharp
var factory = new UserControlFactory();

var profileContent = factory.CreateUserControl<ProfileContent>(pageName =>
{
    NavigateToPage(pageName);
});

profileContent.Open();
contentArea.Content = profileContent;
```

### Statistics Loading
```csharp
public async Task LoadStatisticAsync(Grid grid)
{
    Di.Container.ResolveFieldsFromClassInstance(this);
    
    var games = await _dbService.Where<GameProgresses>(e =>
        e.PlayerID == currentUserId);
    
    int finishedCount = games.Count(e => e.IsFinished);
    
    FactoryNewCard(grid, new StatisticCardInfo("Games count", games.Count.ToString(), 0, 0));
    FactoryNewCard(grid, new StatisticCardInfo("Finished", finishedCount.ToString(), 0, 1));
}
```

### Opening Games Table
```csharp
private void OpenTable(object? sender, RoutedEventArgs e)
{
    var tableWindow = new GamesTableWindow();
    tableWindow.Open();
}
```

## Visual Features

### Animated Borders

**Avatar Border** (1 second loop):
- Rotating conic gradient
- Light blue → Slate gray

**Statistics Header** (3 seconds loop):
- Rotating conic gradient
- Light blue → Slate gray

**Profile Buttons Hover** (2 seconds loop):
- Rotating conic gradient
- Purple shades

### Rounded Corners
- Avatar: 30px radius
- Top container: 20px radius
- Buttons: 30px radius
- Statistic cards: 10px radius

## Integration Points

### Steam API
- SteamFriends.GetPersonaName() - Player nickname
- SteamFriends.GetLargeFriendAvatar() - Avatar image
- SteamManager.GetSteamId() - User identification

### Database
- GameProgresses table - Game history
- Filtered by PlayerID (Steam ID)

### Navigation
- Close button → "Main" page
- Activity game button (not implemented)

## Features

- **Steam Integration**: Automatic avatar and nickname loading
- **Dynamic Statistics**: Real-time calculation from database
- **Animated UI**: Multiple rotating gradient effects
- **Responsive Layout**: Min/max size constraints
- **Data Grid**: Sortable, resizable columns
- **Resource Management**: Proper disposal and cleanup
- **Error Handling**: ErrorService integration
- **MVVM Pattern**: Separation of concerns

## Current State

### Implemented
- Profile layout and styling
- Steam avatar loading
- Statistics card generation
- Games table window
- Animated visual effects
- Database integration
- Resource cleanup

### TODO
- Activity game button functionality
- ProfileViewModel implementation
- More statistics cards
- Filtering/sorting in games table
- Export games list
- Profile editing

## Limitations

- ProfileViewModel is empty
- Activity game button not connected
- Only 2 statistic cards implemented
- No pagination in games table
- No search/filter in table
- Hardcoded design mode data
- No profile editing capabilities
- No avatar upload

## Potential Improvements

```csharp
// Enhanced StatisticViewModel
public async Task LoadStatisticAsync(Grid grid)
{
    var games = await _dbService.Where<GameProgresses>(e =>
        e.PlayerID == currentUserId);
    
    if (games == null) return;
    
    int total = games.Count;
    int finished = games.Count(e => e.IsFinished);
    int inProgress = total - finished;
    double avgGrade = games.Average(e => e.Grade);
    var totalTime = games.Sum(e => (e.EndTime - e.BeginTime).TotalHours);
    var favoriteGenre = GetMostPlayedGenre(games);
    
    FactoryNewCard(grid, new StatisticCardInfo("Total Games", total.ToString(), 0, 0));
    FactoryNewCard(grid, new StatisticCardInfo("Finished", finished.ToString(), 0, 1));
    FactoryNewCard(grid, new StatisticCardInfo("In Progress", inProgress.ToString(), 0, 2));
    FactoryNewCard(grid, new StatisticCardInfo("Avg Grade", avgGrade.ToString("F1"), 1, 0));
    FactoryNewCard(grid, new StatisticCardInfo("Total Hours", totalTime.ToString("F0"), 1, 1));
    FactoryNewCard(grid, new StatisticCardInfo("Favorite Genre", favoriteGenre, 1, 2));
}

// Enhanced GamesTableWindow with filtering
public class GamesTableWindow : Window
{
    private string _searchText = "";
    
    public void FilterGames(string searchText)
    {
        _searchText = searchText;
        
        if (DataContext is StatisticGameTableViewModel vm)
        {
            var filtered = vm.GameProgresses
                .Where(g => g.GameName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            vm.GameProgresses = new ObservableCollection<ProfileTableData>(filtered);
        }
    }
}
```

## Testing

```csharp
[Test]
public async Task TestStatisticsLoading()
{
    var viewModel = new StatisticViewModel();
    var grid = new Grid();
    
    await viewModel.LoadStatisticAsync(grid);
    
    Assert.IsTrue(grid.Children.Count > 0);
}

[Test]
public async Task TestGamesTableLoading()
{
    var viewModel = new StatisticGameTableViewModel();
    await viewModel.LoadTable();
    
    Assert.IsNotNull(viewModel.GameProgresses);
    Assert.IsTrue(viewModel.GameProgresses.Count > 0);
}
```
