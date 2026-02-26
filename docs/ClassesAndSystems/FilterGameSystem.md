# FilterGame System

## Overview
Modal window system for configuring game selection filters by categories, genres, and release years.

## Files
- `FilterGameViewModel.cs` - ViewModel with filter data
- `FilterGameWindow.axaml` - UI layout
- `FilterGameWindow.axaml.cs` - Code-behind logic

## Purpose
- Configure game selection criteria
- Multi-select categories, genres, and years
- Validate games against selected filters
- Provide random year selection from filtered list

## ViewModel (FilterGameViewModel)

### Collections

#### Categories (ObservableCollection<string>)
Game modes and Steam features:
- **Game Modes**: Single-player, Multi-player, Co-op, MMO, PvP, etc.
- **Steam Features**: Achievements, Cloud, Trading Cards, Workshop, etc.
- **Additional**: Captions, Commentary, Stats, Level Editor, etc.

**Total**: 47 predefined categories

#### Genres (ObservableCollection<string>)
Game genres:
- Action, Adventure, Casual, Free to Play, Indie
- Massively Multiplayer, Racing, RPG, Simulation
- Sports, Strategy, Early Access

**Total**: 12 genres

#### Years (List<int>)
Release years from 2003 to current year.

**Generation**: `Enumerable.Range(2003, DateTime.Now.Year - 2003 + 1)`

### Selected Items

#### SelectedCategories (List<string>)
User-selected category filters.

#### SelectedGenres (List<string>)
User-selected genre filters.

#### SelectedYears (List<int>)
User-selected year filters.

### Methods

#### GetCategory() → FilteredData
Returns current filter selections.

**Returns**: FilteredData object with all selections

#### Dispose()
Clears all selected items.

## FilteredData Class

### Purpose
Container for filter selections.

### Properties
```csharp
List<string> Categories
List<string> Genres
List<int> Years
```

### Constructor
```csharp
FilteredData(List<string> categories, List<string> genres, List<int> years)
```

## Window (FilterGameWindow)

### UI Structure

#### Header
- Title: "Фильтры игр"
- Close button (✕)

#### Content Grid
Three sections with labels and multi-select ListBoxes:
1. **Categories** - Max height 200
2. **Genres** - Max height 150
3. **Years** - Max height 150

### Window Properties
- Size: 800x600
- Min: 600x500
- Max: 1000x750
- Background: Dark theme (#1E1E1E)

### Methods

#### Close(object? sender, RoutedEventArgs e)
Closes filter window.

#### CheckFilters(AppSavedContext apps) → bool
Validates game against selected filters.

**Parameters**:
- `apps` - Game context to validate

**Logic**:
1. Get selected filters from ViewModel
2. If categories selected, check if game has any matching category
3. If genres selected, check if game has any matching genre
4. Return true if all checks pass

**Returns**: 
- `true` - Game matches filters
- `false` - Game doesn't match or ViewModel unavailable

**Example**:
```csharp
if (filterWindow.CheckFilters(gameContext))
{
    // Game passes filters
    AddToResults(gameContext);
}
```

#### GetYear() → int
Returns random year from selected years or default range.

**Logic**:
1. Get selected years from ViewModel
2. If years selected, return random from selection
3. Otherwise return random year 2003-2026

**Returns**: Random year (int)

**Example**:
```csharp
int year = filterWindow.GetYear();
var game = gameService.GetRandomGame(year);
```

## Styles

### ListBox.FilteredBox
- Background: #2D2D30
- Border: #3F3F46, thickness 1

### ListBoxItem.FilteredBox
- Padding: 8,4

### ListBoxItem:selected
- Background: #C1185A (pink)

### TextBlock.FilteredBox
- Foreground: GhostWhite

### ListBox TextBlock
- Foreground: PaleVioletRed

## Usage Pattern

### Configuration
```csharp
var filterWindow = new FilterGameWindow();
filterWindow.Open();

// User selects filters and closes window
```

### Game Generation with Filters
```csharp
// Check if filtering enabled
if (filterCheckBox.IsChecked == true)
{
    // Get random year from filter
    var year = filterWindow.GetYear();
    var game = gameService.GetRandomGame(year);
    
    // Validate game against filters
    if (!filterWindow.CheckFilters(game))
        continue; // Skip this game
    
    // Game passes filters
    AddGame(game);
}
```

### Complete Integration
```csharp
while (games.Count < targetCount && iterations < maxIterations)
{
    var year = useFilters 
        ? filterWindow.GetYear() 
        : Random.Shared.Next(2003, 2026);
    
    var game = GetRandomGame(year);
    
    if (game == null) continue;
    
    if (useFilters && !filterWindow.CheckFilters(game))
        continue;
    
    games.Add(game);
    iterations++;
}
```

## Filter Logic

### Category Matching
Uses `Any()` to check if game has at least one selected category:
```csharp
selectedCategories.Any(c => game.Categories.Contains(c))
```

### Genre Matching
Uses `Any()` to check if game has at least one selected genre:
```csharp
selectedGenres.Any(g => game.Genres.Contains(g))
```

### Year Selection
Random selection from filtered years or default range.

## Integration with RollGame

RollGame uses FilterGameWindow for:
1. **Filter Configuration**: User opens window via "Filters" button
2. **Year Selection**: GetYear() provides filtered random year
3. **Game Validation**: CheckFilters() validates generated games

**Flow**:
```
User clicks "Filters" → Configure selections → Close window
User enables filter checkbox → Click "Search"
→ For each game: GetYear() → Generate → CheckFilters() → Add if valid
```

## Best Practices

1. **Open Once**: Create window instance once, reuse for multiple generations
2. **Check Enabled**: Only call filter methods when filtering is enabled
3. **Validate Results**: Always check CheckFilters() return value
4. **Handle Empty**: GetYear() handles empty selection gracefully
5. **Dispose ViewModel**: Call Dispose() when done with filters

## Features

- **Multi-Select**: Select multiple categories, genres, and years
- **Visual Feedback**: Selected items highlighted in pink
- **Flexible Filtering**: Empty selection = no filter for that category
- **Year Range**: Automatically includes current year
- **Dark Theme**: Consistent with application style
- **Reusable**: Window can be opened/closed multiple times

## Limitations

- No "Select All" / "Clear All" buttons
- No filter persistence between sessions
- No filter presets/saving
- Categories and genres are hardcoded
- Year range starts at 2003 (hardcoded)
