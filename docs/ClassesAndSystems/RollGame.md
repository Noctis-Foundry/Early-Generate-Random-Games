# RollGame

## Overview
User control for random game selection interface. Displays dynamically generated game options with filtering capabilities using MVVM pattern.

## Files
- `RollGame.axaml` - UI layout
- `RollGame.axaml.cs` - Code-behind with UI logic
- `RollGameViewModel.cs` - Business logic and data management

## Purpose
- Random game generation from Steam library
- Visual game selection interface with dynamic grid
- Filter integration for targeted selection
- MVVM architecture with separated concerns

## UI Structure

### Header Section
- Title: "GAME RANDOMIZER"
- Filter button: Opens FilterGameWindow
- Close button: Returns to main content

### Content Panel
- Dynamic game grid (up to 4 games)
- Game images with click handlers
- Control panel with:
  - Search button
  - Count input (1-4)
  - Filter checkbox (bound to ViewModel)

## Architecture

### View (RollGame.axaml.cs)
**Properties**:
```csharp
private ErrorService? _errorService
private ConfirmService? _confirmDialog
private ChooseGameWindow? _chooseGameWindow
private FilterGameWindow? _filterGameWindow
private List<RollButtonsInfo> _buttonsInfo
private GifImage? _loadGif
private SemaphoreSlim _rollSemaphore
```

**Constants**:
- `DefaultCountApp = 1`
- `IterationDelayMilliseconds = 500`
- `maxCountGames = 4`

### ViewModel (RollGameViewModel)
**Properties**:
```csharp
private List<AppInfo> _appInfo
private IGenApp? _generateRandomApps
private bool _isFilter
private int _iterationCount
```

**Constants**:
- `IterationLimit = 500`

## Methods

### View Methods (RollGame.axaml.cs)

#### GenerateGame(object? sender, RoutedEventArgs e)
Event handler for game generation button click.

**Process**:
1. Check semaphore availability (non-blocking)
2. Show error if generation already in progress
3. Parse game count from CountApp TextBox
4. Setup grid layout via SetupGrid()
5. Get filters from FilterGameWindow
6. Call ViewModel.GenerateGames()
7. Generate UI elements via GenerateUi()
8. Release semaphore in finally block

**Thread Safety**: Uses SemaphoreSlim to prevent concurrent operations

#### GenerateUi()
Creates UI elements for generated games with animation delay.

**Process**:
1. Get MainWindowFactory instance
2. Iterate through ViewModel.AppInfo
3. Create button/image grid elements
4. Initialize components via InitDictionaryWithComponents()
5. Delay 500ms between iterations for visual effect
6. Remove loading GIF when complete

#### SetupGrid(int countGames)
Configures grid layout for specified game count.

**Actions**:
- Clears existing grid children and column definitions
- Creates column definitions for each game
- Adds animated loading GIF to grid

#### InitDictionaryWithComponents(GridElements, AppInfo)
Associates game data with UI components.

**Process**:
1. Convert image bytes to Bitmap
2. Set image source
3. Create RelayCommand for button click
4. Command opens ChooseGameWindow with game data
5. Store button info in _buttonsInfo list

#### TextBoxEventsInit()
Configures count input validation.

**Validation**: Clamps input to 1-4 range (maxCountGames)

#### GoToFilter(object? sender, RoutedEventArgs e)
Opens filter configuration window.

#### Close(object? sender, RoutedEventArgs e)
Navigates back to main content and disposes ViewModel.

**Process**:
1. Invoke navigation action to "Main"
2. Dispose ViewModel resources

### ViewModel Methods (RollGameViewModel)

#### GenerateGames(int countGames, FilteredData? filteredGamesData, CancellationToken cancellationToken)
Main game generation logic.

**Parameters**:
- `countGames` - Number of games to generate (1-4)
- `filteredGamesData` - Optional filter criteria
- `cancellationToken` - Cancellation support

**Process**:
1. Validate initialization via IsValidationGenerateData()
2. Clear previous data
3. Check cancellation token
4. Loop until IterationLimit (500) or countGames reached:
   - Get random game from IGenApp
   - Skip if null or duplicate AppId
   - Apply filters if IsFilter enabled
   - Load game image bytes asynchronously
   - Add to _appInfo list
5. Handle exceptions

**Iteration Limit**: 500 attempts to prevent infinite loops

#### FilterGame(AppSavedContext savedGame, FilteredData filter)
Checks if game matches filter criteria.

**Returns**: `bool` - True if game passes all filters

**Filter Logic**:
- Categories: Game must contain at least one selected category
- Genres: Game must contain at least one selected genre
- Years: Game release year must match selected years

#### IsValidationGenerateData()
Validates that game generator is initialized.

**Returns**: `bool` - _generateRandomApps.IsInitialized

#### ClearItems()
Resets generation state.

**Actions**:
- Clears _appInfo list
- Resets _iterationCount to 0

## UI Components

### Layout Structure
```
DockPanel (Background: RandomGame.png)
├── Border.HeaderBorder (Top)
│   └── Grid
│       ├── TextBlock "GAME RANDOMIZER"
│       ├── Button "Filters" (GoToFilter)
│       └── Button "✕" (Close)
├── StackPanel.ContentPanel
│   ├── Grid (GamesGrid) - Dynamic columns
│   │   └── Border.GameBorder × N
│   │       └── Button.RandomButton
│   │           └── Image.GameImages
│   └── Border.ContentBorder
│       └── Grid
│           ├── Button.GenerateButton "Search"
│           ├── Border.InputBorder
│           │   └── MaskedTextBox (CountApp)
│           └── CheckBox "filter" (IsFilter binding)
```

### Key Style Classes
- **HeaderBorder**: Top panel with gradient border
- **CloseButton**: Close button with hover animation
- **ContentPanel**: Main content container
- **GameBorder**: Individual game card border
- **RandomButton**: Transparent button for game selection
- **GameImages**: Game cover images
- **GenerateButton**: Search button (Height: 60)
- **InputBorder**: Count input container (60×60)
- **CountInput**: Numeric input (Font: 36, bold)

## Data Flow

### Generation Flow
```
User clicks Search
  ↓
View: GenerateGame() - Acquire semaphore
  ↓
View: SetupGrid() - Configure layout
  ↓
ViewModel: GenerateGames() - Generate game data
  ↓
View: GenerateUi() - Create UI elements
  ↓
View: Release semaphore
```

### Filter Flow
```
ViewModel.IsFilter = true (CheckBox binding)
  ↓
GenerateGames() checks IsFilter
  ↓
FilterGame() validates each game
  ↓
Only matching games added to AppInfo
```

## Usage Flow

1. User opens RollGame control
2. Optionally configures filters via "Filters" button
3. Sets game count (1-4) in input field
4. Checks filter checkbox if filtering desired
5. Clicks "Search" button
6. Loading GIF displays during generation
7. Games appear with 500ms delay between each
8. User clicks game image to view details in ChooseGameWindow

## Integration Points

### View Dependencies (Injected)
- `ErrorService` - Error message display
- `ConfirmService` - User confirmation dialogs

### View Dependencies (Instantiated)
- `ChooseGameWindow` - Game details display
- `FilterGameWindow` - Filter configuration
- `MainWindowFactory` - Grid layout and GIF creation
- `SteamService` - Image conversion

### ViewModel Dependencies
- `IGenApp` (GenerateRandomApps) - Random game generation
- `SteamService` - Image byte loading

### Navigation
- Extends `MainWindowUserControlAbstract`
- Uses `_changeWindowAction` for navigation
- Navigates to "Main" on close

### Data Binding
- `IsFilter` property bound to CheckBox
- Two-way binding for filter state

## Error Handling

### View Level
- Shows error if generation already in progress
- Throws NullReferenceException if services not injected
- Throws NullReferenceException if bitmap conversion fails
- Always releases semaphore in finally block

### ViewModel Level
- Validates initialization before generation
- Skips games with null image bytes
- Catches and logs all exceptions during generation
- Supports cancellation token (though not currently used)

## Thread Safety

### Concurrency Control
- **SemaphoreSlim**: Prevents multiple simultaneous operations
- **Non-blocking Check**: Uses `WaitAsync(0)` to check availability
- **Error Display**: Shows error message if generation in progress

### Current Behavior
```
User clicks Generate (operation in progress) →
Semaphore unavailable →
Show error message →
Return without starting new operation
```

### Resource Cleanup
- Semaphore released in finally block
- ViewModel disposed on close

## Disposal

### View Disposal
- Calls ViewModel.Dispose() on close
- Retains window references (not disposed)

### ViewModel Disposal
- Clears _generateRandomApps reference
- Clears _appInfo list
- Resets _iterationCount

## Key Features

1. **MVVM Separation**: Business logic in ViewModel, UI logic in View
2. **Thread Safety**: Semaphore prevents concurrent generation
3. **Visual Feedback**: Loading GIF and staggered game appearance
4. **Flexible Filtering**: Optional category/genre/year filters
5. **Iteration Limit**: Prevents infinite loops (500 attempts)
6. **Dynamic Layout**: Grid adjusts to game count (1-4)
7. **Dependency Injection**: Services injected via Di.Container
8. **Data Binding**: IsFilter property bound to CheckBox

## Best Practices

1. Use semaphore for concurrent operation control
2. Release semaphore in finally block
3. Validate count input (1-4 range)
4. Use iteration limit to prevent infinite loops
5. Handle null image bytes gracefully
6. Dispose ViewModel on close
7. Separate UI and business logic (MVVM)
8. Use RelayCommand for button actions
