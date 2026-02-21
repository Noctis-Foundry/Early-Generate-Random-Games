# Game Generation System

## Overview
System for generating random game selections from a pre-loaded catalog. Organizes games by release year and supports filtering by categories.

---

## AppSavedContext

### Purpose
Data model representing a Steam game with metadata.

### Properties

- `AppId` (int) - Steam application ID
- `AppReleaseYear` (int) - Year of release
- `AppName` (string) - Game title
- `AppDescription` (string) - Game description
- `HeaderImage` (string) - URL to header image
- `AppGenres` (Dictionary<int, string>) - Genre ID to name mapping
- `AppCategoris` (Dictionary<int, string>) - Category ID to name mapping

### Constructor
```csharp
AppSavedContext(int appId, string appName, string appDescription, 
                int appReleaseYear, Dictionary<int, string> appGenres, 
                Dictionary<int, string> appCategoris, string headerImage)
```

### Usage
```csharp
var game = new AppSavedContext(
    appId: 570,
    appName: "Dota 2",
    appDescription: "MOBA game",
    appReleaseYear: 2013,
    appGenres: new Dictionary<int, string> { {1, "Action"} },
    appCategoris: new Dictionary<int, string> { {2, "Multiplayer"} },
    headerImage: "https://..."
);
```

---

## GenerateRandomApps

### Purpose
Service for loading and randomly selecting games from JSON catalog. Implements IGenApp interface.

### Interface: IGenApp
```csharp
bool IsInitialized { get; }
AppSavedContext? GetRandomGame(int year);
AppSavedContext? GetRandomGame(int year, int indexCategory);
```

### Properties

**IsInitialized** (bool)
Indicates whether game catalog loaded successfully.

### Private Fields

- `_apps` - Dictionary mapping release year to game list
- `_localPath` - Path to JSON file: `Assets/Jsons/temp_apps.json`
- `_rng` - Random number generator

### Constructor

**GenerateRandomApps()**

**Process**:
1. Constructs path to `temp_apps.json`
2. Validates file exists
3. Loads and parses JSON
4. Organizes games by release year
5. Sets IsInitialized to true

**Exceptions**:
- `FileNotFoundException` - JSON file not found
- `Exception` - JSON parsing or dictionary conversion failed

### Methods

#### GetRandomGame(int year)
Returns random game from specified year.

**Parameters**:
- `year` - Release year filter

**Returns**: `AppSavedContext?` - Random game or null if year not found

**Algorithm**:
1. Check if year exists in catalog
2. Get game list for year
3. Return random game using RNG

**Example**:
```csharp
var game = genApps.GetRandomGame(2020);
if (game != null)
{
    Console.WriteLine($"Selected: {game.AppName}");
}
```

#### GetRandomGame(int year, int indexCategory)
Returns first game matching year and category.

**Parameters**:
- `year` - Release year filter
- `indexCategory` - Category ID filter

**Returns**: `AppSavedContext?` - Matching game or null

**Algorithm**:
1. Get games for specified year
2. Find first game with matching category ID
3. Return result

**Note**: Returns first match, not random selection

**Example**:
```csharp
var multiplayerGame = genApps.GetRandomGame(2020, categoryId: 2);
```

### Private Methods

#### GetAppList()
Loads and parses JSON file into internal dictionary structure.

**Process**:
1. Read JSON file from disk
2. Deserialize to List<AppSavedContext>
3. Group games by release year
4. Populate _apps dictionary

**JSON Format**:
```json
[
  {
    "appId": 570,
    "appName": "Dota 2",
    "appDescription": "...",
    "appReleaseYear": 2013,
    "appGenres": {"1": "Action"},
    "appCategoris": {"2": "Multiplayer"},
    "headerImage": "https://..."
  }
]
```

**Error Handling**:
- Validates file exists
- Checks JSON not empty
- Ensures deserialization succeeds
- Throws descriptive exceptions on failure

---

## Data Structure

### Internal Organization
```
_apps: Dictionary<int, List<AppSavedContext>>
{
    2020: [game1, game2, game3],
    2021: [game4, game5],
    2022: [game6]
}
```

**Benefits**:
- Fast year-based lookup: O(1)
- Efficient random selection within year
- Memory-efficient grouping

---

## Usage Example

### Initialization
```csharp
try
{
    var genApps = new GenerateRandomApps();
    
    if (genApps.IsInitialized)
    {
        Console.WriteLine("Game catalog loaded");
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Random Selection
```csharp
// Get any game from 2020
var game2020 = genApps.GetRandomGame(2020);

// Get multiplayer game from 2020
var multiplayerGame = genApps.GetRandomGame(2020, categoryId: 2);

// Display result
if (game2020 != null)
{
    Console.WriteLine($"Game: {game2020.AppName}");
    Console.WriteLine($"Year: {game2020.AppReleaseYear}");
    Console.WriteLine($"Description: {game2020.AppDescription}");
}
```

---

## File Location
**JSON Path**: `[BaseDirectory]/Assets/Jsons/temp_apps.json`

**BaseDirectory**: Application executable directory

---

## Features

- **Year-based Organization**: Fast filtering by release year
- **Category Filtering**: Find games by category ID
- **Random Selection**: Built-in RNG for game selection
- **Lazy Loading**: Games loaded once during initialization
- **Null Safety**: Returns null for invalid queries
- **Validation**: Comprehensive error checking during load

## Limitations

- Category filtering returns first match, not random
- Entire catalog loaded into memory
- No support for genre filtering
- Single JSON file source
- No dynamic catalog updates

---

## RollGame View

### Purpose
User interface for generating and displaying random game selections with manga-style theming.

### Layout
- Size: 1000x800
- Background: RandomGame.png with black overlay (60% opacity)
- Manga-inspired black/white theme

### UI Structure

**Header**:
- Title: "GAME RANDOMIZER"
- Black background with white border
- Bold Arial font, size 24

**Main Content**:
- Semi-transparent black background (60%)
- White borders throughout
- Close button (top-right)

**Games Grid**:
- 3-column grid (configurable via CountApp)
- Each game in bordered container
- 80x80 game images
- Transparent buttons with white borders

**Control Panel**:
- "Search" button to generate games
- Number input (1-5 games, default 3)
- Black background with white borders
- 60x60 input box

### Code-behind (RollGame.axaml.cs)

**Fields**:
- `_errorService` - Error display service
- `_appData` - Dictionary mapping buttons to game data
- `_random` - Random number generator
- `_generateRandomApps` - Game generation service
- `_mainWindowFactory` - UI factory for dynamic grid
- `_isRolling` - Prevents concurrent generation
- `_lastYear` - Avoids duplicate years

**Methods**:

**GenerateGames(object sender, RoutedEventArgs e)**
- Validates initialization state
- Parses game count from input (1-5)
- Generates random years (2010-2025)
- Avoids duplicate years
- Fetches games from catalog
- Creates dynamic grid with MainWindowFactory
- Loads game images from Steam
- Initializes button click handlers

**InitDictionaryWithComponents(List<Button>, List<Image>, List<AppSavedContext?>)**
- Maps buttons to game data
- Downloads game header images
- Populates dictionary
- Error handling for duplicates

**InitializeButtonListeners()**
- Attaches click handlers to game buttons
- Uses RelayCommand with ViewModel
- Calls ViewModel.ChooseGame() on click

**TextBoxEventsInit()**
- Validates number input (1-5 range)
- Clamps values automatically
- Updates text on invalid input

**Close(object? sender, RoutedEventArgs e)**
- Navigates to "Main" page
- Calls Dispose() for cleanup

**Dispose()**
- Nulls all references
- Clears dictionary
- Releases resources

### Features

- **Dynamic Grid**: Adjusts to game count (1-5)
- **Year Filtering**: Random years 2010-2025
- **Duplicate Prevention**: No repeated years in single generation
- **Image Loading**: Async Steam image downloads
- **Input Validation**: Automatic clamping to valid range
- **Manga Theme**: Black/white aesthetic with borders
- **IUserControl**: Compatible with navigation system

### Integration

```csharp
var factory = new UserControlFactory();
var rollGame = factory.CreateUserControl<RollGame>(pageName =>
{
    NavigateToPage(pageName);
});

rollGame.Open();
contentArea.Content = rollGame;
```

### Workflow

```
User clicks "Search" → Parse count → Generate random years → 
Fetch games → Create grid → Load images → Display → 
User clicks game → ViewModel.ChooseGame()
```

### Limitations

- Fixed year range (2010-2025)
- Max 5 games per generation
- No category filtering in UI
- No loading indicator during image download
- Hardcoded manga theme

---

## ChooseGameWindow

### Purpose
Modal window displaying detailed game information after user selects a game from RollGame view.

### Layout
- Size: 400x300
- Uses GameInfoContainer style
- 2-column grid layout

### UI Elements

**Left Column** (Game Information):
- **GameName** - Game title with "Name: " prefix
- **GameGenres** - Genre list with "Genres: " prefix
- **ReleaseDate** - Release year with "Release: " prefix
- **GameRating** - Rating with "Rating: " prefix
- **GameDevelopers** - Developer names with "Developers: " prefix

**Right Column**:
- **Game Image** - 140px width, spans 4 rows
- **Action Buttons** (row 5):
  - "Steam" - Opens Steam store page
  - "Choose Game" - Confirms game selection

### Styling
- Uses GameInfoContainer, GameInfoGrid, GameInfoItem classes
- GameImageBorder with animated gradient
- GameInfoButton style for action buttons
- GameInfoText for labels

### Code-behind

Minimal implementation with only InitializeComponent().

### Integration

```csharp
// Called from RollGameViewModel.ChooseGame()
public async Task ChooseGame(AppSavedContext game)
{
    var chooseWindow = new ChooseGameWindow();
    
    // Populate game data
    chooseWindow.GameName.Text = $"Name: {game.AppName}";
    chooseWindow.GameGenres.Text = $"Genres: {string.Join(", ", game.AppGenres.Values)}";
    chooseWindow.ReleaseDate.Text = $"Release: {game.AppReleaseYear}";
    
    await chooseWindow.ShowDialog(mainWindow);
}
```

### Features
- **Detailed View**: Shows comprehensive game information
- **Action Buttons**: Steam integration and game confirmation
- **Consistent Styling**: Matches CurrentGameStatus design
- **Modal Dialog**: Blocks interaction until closed

### Limitations
- No data binding (manual population required)
- Button actions not implemented
- Static placeholder image
- No ViewModel
- Hardcoded example text
