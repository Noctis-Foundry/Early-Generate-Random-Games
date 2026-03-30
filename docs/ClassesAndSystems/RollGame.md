# RollGame System

## Overview
Random game selection system with filtering capabilities. Generates 1-5 random games from Steam library with optional category, genre, and year filters. Uses MVVM pattern with separated ViewModel logic.

## Purpose
- Generate random games from Steam library
- Apply optional filters (categories, genres, years)
- Display game selection interface
- Prevent duplicate game selection
- Limit generation attempts to prevent infinite loops

## ViewModel (RollGameViewModel)

**Namespace**: `GameRandom.ViewModels.AdminConfirmSystem`

**Inheritance**: Extends `ViewModelBase`

### Injected Dependencies
- `SteamService` - Image loading from URLs

### Constructor Parameters
- `IGenApp generateRandomApps` - Game generation service

### Properties

#### AppInfo (List<AppInfo>)
List of generated games with data and images.

**Access**: Read-only property exposing private `_appInfo` list

#### IsFilter (bool)
Flag indicating whether filtering is enabled.

**Binding**: Two-way data binding with UI CheckBox

**Default**: false

### Constants

**IterationLimit = 500**
Maximum attempts to find suitable games before stopping.

**Purpose**: Prevents infinite loops when filters are too restrictive

### Core Methods

#### GenerateGames(int countGames, FilteredData? filteredGamesData, CancellationToken cancellationToken)
Generates specified number of random games with optional filtering.

**Parameters**:
- `countGames` - Number of games to generate (1-5)
- `filteredGamesData` - Optional filter criteria (categories, genres, years)
- `cancellationToken` - Cancellation support

**Process**:
1. Validates `_generateRandomApps` is initialized
2. Clears previous results via ClearItems()
3. Loops up to IterationLimit (500) or until countGames reached:
   - Checks cancellation token
   - Calls GenerateAppInfo() for single game
   - Adds non-null results to _appInfo list
4. Catches and logs exceptions

**Iteration Logic**:
```csharp
for (int i = 0; i < IterationLimit && _appInfo.Count < countGames; i++)
```

**Early Exit**: Returns immediately if generator not initialized

#### GenerateAppInfo(FilteredData? filteredGamesData) → Task<AppInfo?>
Generates single random game with validation and filtering.

**Returns**: AppInfo object or null if game rejected

**Process**:
1. Get random game from `_generateRandomApps.GetRandomGame()`
2. Skip if null or duplicate AppId
3. Apply filters if IsFilter enabled
4. Load image bytes from Steam via `_steamService.GetImageBytes()`
5. Skip if image loading fails
6. Create and return AppInfo object

**Duplicate Prevention**:
```csharp
if (_appInfo.Any(e => e.AppData.AppId == gameInfo.AppId))
    return null;
```

**Filter Application**:
```csharp
if (IsFilter && filteredGamesData is not null)
    if (!FilterGame(gameInfo, filteredGamesData))
        return null;
```

#### FilterGame(AppSavedContext savedGame, FilteredData filter) → bool
Validates game against filter criteria.

**Parameters**:
- `savedGame` - Game data to validate
- `filter` - Active filter criteria

**Returns**: true if game passes all filters, false otherwise

**Filter Logic** (AND between types, OR within type):

**Categories**:
```csharp
if (filter.Categories.Count > 0 && !filter.Categories.Any(c => savedGame.AppCategories.Contains(c)))
    return false;
```
- Empty list = no filter
- Game must have at least one matching category

**Genres**:
```csharp
if (filter.Genres.Count > 0 && !filter.Genres.Any(g => savedGame.AppGenres.Contains(g)))
    return false;
```
- Empty list = no filter
- Game must have at least one matching genre

**Years**:
```csharp
if (filter.Years.Count > 0 && !filter.Years.Any(y => y == savedGame.AppReleaseYear))
    return false;
```
- Empty list = no filter
- Game release year must match one of selected years

#### ClearItems() (private)
Resets generation state.

**Actions**:
- Clears `_appInfo` list
- Resets `_iterationCount` to 0

#### Dispose() (override)
Cleanup resources.

**Actions**:
- Calls ClearItems()
- Nulls `_generateRandomApps` reference
- Calls base.Dispose()

## Data Structures

### AppInfo
Container for game data and image.

**Properties**:
- `AppData` (AppSavedContext) - Game metadata
- `ImageBytes` (byte[]) - Game header image

**Constructor**:
```csharp
public AppInfo(AppSavedContext appData, byte[] imageBytes)
```

### AppSavedContext
Game metadata from JSON catalog.

**Properties**:
- `AppId` (int) - Steam application ID
- `AppName` (string) - Game title
- `HeaderImage` (string) - Image URL
- `AppCategories` (List<string>) - Game categories
- `AppGenres` (List<string>) - Game genres
- `AppReleaseYear` (int) - Release year

### FilteredData
Filter criteria container.

**Properties**:
- `Categories` (List<string>) - Selected categories
- `Genres` (List<string>) - Selected genres
- `Years` (List<int>) - Selected years

## Generation Flow

### Standard Generation
```
User triggers generation
  ↓
ViewModel.GenerateGames(count, filters)
  ↓
ClearItems() - Reset state
  ↓
Loop (max 500 iterations):
  ↓
  GenerateAppInfo(filters)
    ↓
    Get random game from IGenApp
    ↓
    Check for duplicates
    ↓
    Apply filters (if enabled)
    ↓
    Load image bytes
    ↓
    Create AppInfo
  ↓
  Add to _appInfo list
  ↓
  Check if count reached
  ↓
Return results
```

### With Filtering
```
IsFilter = true
  ↓
GenerateAppInfo() calls FilterGame()
  ↓
Check categories (OR logic)
  ↓
Check genres (OR logic)
  ↓
Check years (exact match)
  ↓
Return true/false
  ↓
If false: Skip game, continue loop
If true: Load image and add to results
```

## Usage Example

### Basic Generation
```csharp
var genApp = new GenerateRandomApps();
var viewModel = new RollGameViewModel(genApp);

// Generate 3 random games without filters
await viewModel.GenerateGames(3, null);

// Access results
foreach (var game in viewModel.AppInfo)
{
    Console.WriteLine($"{game.AppData.AppName} ({game.AppData.AppId})");
}
```

### With Filtering
```csharp
var viewModel = new RollGameViewModel(genApp);
viewModel.IsFilter = true;

var filters = new FilteredData(
    categories: new List<string> { "Single-player", "Achievements" },
    genres: new List<string> { "Action", "RPG" },
    years: new List<int> { 2020, 2021, 2022 }
);

await viewModel.GenerateGames(5, filters);
```

### Cancellation Support
```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10));

try
{
    await viewModel.GenerateGames(5, filters, cts.Token);
}
catch (OperationCanceledException)
{
    Logger.Info("Generation cancelled");
}
```

## Integration Points

### IGenApp Interface
Game generation service providing random game selection.

**Required Methods**:
- `GetRandomGame()` → AppSavedContext - Returns random game from catalog
- `IsInitialized` (property) - Indicates if service is ready

### SteamService
Image loading service.

**Used Methods**:
- `GetImageBytes(string url)` → Task<byte[]> - Downloads image from URL

### FilterGameViewModel
Provides filter data via GetFilters() method.

**Integration**:
```csharp
var filterViewModel = new FilterGameViewModel();
var filters = filterViewModel.GetFilters();
await rollGameViewModel.GenerateGames(count, filters);
```

## Error Handling

### Initialization Validation
```csharp
if (_generateRandomApps is null || !_generateRandomApps.IsInitialized)
    return;
```
- Returns early if generator not ready
- No exception thrown

### Null Handling
- Null games from generator are skipped
- Null image bytes cause game to be skipped
- No exceptions thrown for individual game failures

### Exception Catching
```csharp
try
{
    // Generation loop
}
catch (Exception e)
{
    Logger.Error("Failed to generate games: " + e.Message);
}
```
- Catches all exceptions during generation
- Logs error message
- Returns partial results if any games generated

### Cancellation
```csharp
cancellationToken.ThrowIfCancellationRequested();
```
- Checks cancellation token each iteration
- Throws OperationCanceledException if cancelled

## Features

- **Duplicate Prevention**: Checks AppId before adding games
- **Iteration Limit**: Maximum 500 attempts prevents infinite loops
- **Flexible Filtering**: Optional category/genre/year filters with OR logic
- **Async Image Loading**: Non-blocking image downloads
- **Cancellation Support**: CancellationToken parameter
- **MVVM Pattern**: Clean separation of concerns
- **Null Safety**: Graceful handling of null results
- **Error Logging**: Comprehensive error reporting
- **Data Binding**: IsFilter property for UI integration

## Performance Considerations

### Iteration Limit
- Prevents infinite loops with restrictive filters
- 500 attempts should be sufficient for most filter combinations
- May return fewer games than requested if limit reached

### Image Loading
- Async loading prevents UI blocking
- Failed image loads skip game (no retry)
- Network errors logged but don't stop generation

### Memory Management
- ClearItems() called before each generation
- Dispose() clears all references
- Image bytes stored in memory (consider caching for large sets)
