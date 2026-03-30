# FilterGame System

## Overview
Game filtering system for configuring selection criteria by categories, genres, and release years. Loads filter options from JSON files and provides data structure for filter validation.

## Files
- `FilterGameViewModel.cs` - ViewModel with filter data and selection management
- `FilteredData.cs` - Data structure for filter selections (embedded in ViewModel file)

## Purpose
- Load available categories and genres from JSON assets
- Manage multi-select filter collections
- Provide filter data structure for game validation
- Generate year range from 2003 to current year

## ViewModel (FilterGameViewModel)

**Namespace**: `GameRandom.ViewModels.BaseClasses`

**Inheritance**: Extends `ViewModelBase`

### Data Sources

#### Categories (ObservableCollection<string>)
Loaded from `Assets/Jsons/categories.json`

**Content**: Game modes and Steam features
- Game Modes: Single-player, Multi-player, Co-op, MMO, PvP
- Steam Features: Achievements, Cloud, Trading Cards, Workshop
- Additional: Captions, Commentary, Stats, Level Editor

**Total**: 47 categories (loaded from JSON)

#### Genres (ObservableCollection<string>)
Loaded from `Assets/Jsons/genres.json`

**Content**: Game genres
- Action, Adventure, Casual, Free to Play, Indie
- Massively Multiplayer, Racing, RPG, Simulation
- Sports, Strategy, Early Access

**Total**: 12 genres (loaded from JSON)

#### Years (List<int>)
Generated programmatically from 2003 to current year.

**Generation**: 
```csharp
Enumerable.Range(2003, DateTime.Now.Year - 2003 + 1).ToList()
```

**Example**: For 2024, generates [2003, 2004, ..., 2024]

### Selected Items

#### SelectedCategories (List<string>)
User-selected category filters.

#### SelectedGenres (List<string>)
User-selected genre filters.

#### SelectedYears (List<int>)
User-selected year filters.

### Methods

#### Constructor
Initializes ViewModel and loads filter data from JSON files.

```csharp
public FilterGameViewModel()
{
    LoadDataFromJson();
}
```

#### LoadDataFromJson() (private)
Loads categories and genres from JSON asset files.

**File Paths**:
- Categories: `Assets/Jsons/categories.json`
- Genres: `Assets/Jsons/genres.json`

**Process**:
1. Constructs file paths using `AppContext.BaseDirectory`
2. Checks file existence
3. Deserializes JSON to `List<string>`
4. Converts to `ObservableCollection<string>`
5. Assigns to Categories/Genres properties

**Error Handling**: 
- Logs error via Logger.Error()
- Throws exception on failure

**Example JSON Structure**:
```json
[
  "Single-player",
  "Multi-player",
  "Co-op",
  "Achievements"
]
```

#### GetFilters() → FilteredData
Returns current filter selections as FilteredData object.

**Returns**: New FilteredData instance with selected items

```csharp
public FilteredData GetFilters()
{
    return new FilteredData(SelectedCategories, SelectedGenres, SelectedYears);
}
```

#### Dispose() (override)
Clears all collections and releases resources.

**Actions**:
1. Clears selected item lists (backing fields and properties)
2. Clears data source collections (Categories, Genres, Years)
3. Calls base.Dispose()

**Cleared Collections**:
- _selectedCategories, SelectedCategories
- _selectedGenres, SelectedGenres
- _selectedYears, SelectedYears
- _categories, Categories
- _genres, Genres
- _years, Years

## FilteredData Class

**Namespace**: `GameRandom.ViewModels.BaseClasses` (same file as ViewModel)

**Type**: Record-style class with primary constructor

### Properties
```csharp
List<string> Categories
List<string> Genres
List<int> Years
```

### Constructor
```csharp
public class FilteredData(List<string> categories, List<string> genres, List<int> years)
```

**Parameters**:
- `categories` - Selected category filters
- `genres` - Selected genre filters
- `years` - Selected year filters

## JSON Asset Files

### categories.json
**Location**: `Assets/Jsons/categories.json`

**Format**: JSON array of strings

**Example**:
```json
[
  "Single-player",
  "Multi-player",
  "Co-op",
  "Achievements",
  "Steam Cloud",
  "Trading Cards"
]
```

### genres.json
**Location**: `Assets/Jsons/genres.json`

**Format**: JSON array of strings

**Example**:
```json
[
  "Action",
  "Adventure",
  "RPG",
  "Strategy",
  "Simulation"
]
```

## Usage Pattern

### Initialization
```csharp
var filterViewModel = new FilterGameViewModel();
// Categories and genres automatically loaded from JSON
// Years automatically generated
```

### User Selection (via UI binding)
```csharp
// User selects items in UI (ListBox multi-select)
filterViewModel.SelectedCategories = new List<string> { "Single-player", "Achievements" };
filterViewModel.SelectedGenres = new List<string> { "Action", "RPG" };
filterViewModel.SelectedYears = new List<int> { 2020, 2021, 2022 };
```

### Retrieving Filter Data
```csharp
var filters = filterViewModel.GetFilters();

// Use filters for game validation
foreach (var game in games)
{
    bool matchesCategories = filters.Categories.Count == 0 || 
        filters.Categories.Any(c => game.Categories.Contains(c));
    
    bool matchesGenres = filters.Genres.Count == 0 || 
        filters.Genres.Any(g => game.Genres.Contains(g));
    
    bool matchesYears = filters.Years.Count == 0 || 
        filters.Years.Contains(game.ReleaseYear);
    
    if (matchesCategories && matchesGenres && matchesYears)
    {
        // Game passes filters
        filteredGames.Add(game);
    }
}
```

## Filter Validation Logic

### Empty Selection Behavior
Empty selection for a filter type means "no filter" for that category:
- Empty Categories → All categories accepted
- Empty Genres → All genres accepted
- Empty Years → All years accepted

### Matching Logic
Uses `Any()` for OR logic within each filter type:

**Category Matching**:
```csharp
bool matchesCategories = selectedCategories.Count == 0 || 
    selectedCategories.Any(c => game.Categories.Contains(c));
```

**Genre Matching**:
```csharp
bool matchesGenres = selectedGenres.Count == 0 || 
    selectedGenres.Any(g => game.Genres.Contains(g));
```

**Year Matching**:
```csharp
bool matchesYears = selectedYears.Count == 0 || 
    selectedYears.Contains(game.ReleaseYear);
```

**Combined Logic** (AND between filter types):
```csharp
bool passesFilters = matchesCategories && matchesGenres && matchesYears;
```

## Integration Points

### RollGame System
RollGame uses FilterGameViewModel for game selection filtering:
1. User configures filter selections via UI
2. RollGame retrieves filters via GetFilters()
3. Generated games validated against filter criteria
4. Only matching games displayed to user

### UI Binding
ViewModel properties designed for XAML data binding:
```xml
<ListBox ItemsSource="{Binding Categories}" 
         SelectedItems="{Binding SelectedCategories}"
         SelectionMode="Multiple"/>

<ListBox ItemsSource="{Binding Genres}" 
         SelectedItems="{Binding SelectedGenres}"
         SelectionMode="Multiple"/>

<ListBox ItemsSource="{Binding Years}" 
         SelectedItems="{Binding SelectedYears}"
         SelectionMode="Multiple"/>
```

## Error Handling

### JSON Loading Errors
- Logs error message via Logger.Error()
- Throws exception to caller
- Includes exception message in log

**Example Error Log**:
```
Error loading filter data: Could not find file 'categories.json'
```

### File Not Found
- Silently skips missing files
- Collections remain empty if JSON not found
- No exception thrown for missing files

## Features

- **JSON-Based Configuration**: Categories and genres loaded from external files
- **Dynamic Year Range**: Automatically includes current year
- **Observable Collections**: UI-friendly data binding support
- **Multi-Select Support**: Lists allow multiple selections
- **Flexible Filtering**: Empty selection = no filter
- **MVVM Pattern**: Clean separation of data and UI
- **Memory Management**: Proper disposal of collections
- **Error Logging**: Comprehensive error reporting

## Best Practices

1. **Dispose Properly**: Call Dispose() when done with ViewModel
2. **Handle Empty Filters**: Check for empty collections before validation
3. **Validate JSON Files**: Ensure categories.json and genres.json exist
4. **Use GetFilters()**: Don't access selected collections directly
5. **Thread Safety**: ViewModel not thread-safe, use on UI thread only

## Limitations

- No filter persistence between sessions
- No filter presets or saving functionality
- Year range hardcoded to start at 2003
- JSON files must exist at startup
- Not thread-safe (UI thread only)
- No validation of JSON content structure
