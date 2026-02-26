# RollGame

## Overview
User control for random game selection interface. Displays dynamically generated game options with filtering capabilities.

## Files
- `RollGame.axaml` - UI layout
- `RollGame.axaml.cs` - Code-behind logic
- `RollGameStyle.axaml` - Style definitions

## Purpose
- Random game generation from Steam library
- Visual game selection interface
- Filter integration for targeted selection
- Dynamic grid layout management

## UI Structure

### Header Section
- Title: "GAME RANDOMIZER"
- Filter button: Opens FilterGameWindow
- Close button: Returns to main content

### Content Panel
- Dynamic game grid (up to 5 games)
- Game images with click handlers
- Control panel with:
  - Search button
  - Count input (1-5)
  - Filter checkbox

## Key Components

### Properties
```csharp
private Dictionary<ButtonContext, AppSavedContext?> _appData
private ChooseGameWindow _chooseGameWindow
private FilterGameWindow _filterGameWindow
private IGenApp _generateRandomApps
private MainWindowFactory _mainWindowFactory
private Action<string>? _onShowContent
private bool _isRolling
```

### Constants
```csharp
private const int MinYear = 2003
private const int MaxYear = 2026
```

## Methods

### GenerateGames(object sender, RoutedEventArgs e)
Main game generation logic.

**Process**:
1. Validate initialization state
2. Parse game count from input
3. Clear previous data and reconfigure grid
4. Generate random games with optional filtering
5. Load game images asynchronously
6. Create UI elements dynamically
7. Initialize button click handlers

**Filtering**:
- If FilterCheckBox is checked, applies year and category/genre filters
- Skips games that don't match filter criteria

**Iteration Limit**: 1000 attempts to prevent infinite loops

### Close(object? sender, RoutedEventArgs e)
Navigates back to main content and disposes resources.

### InitDictionaryWithComponents(Button, Image, AppSavedContext, byte[])
Associates game data with UI components.

**Parameters**:
- `buttons` - Button control
- `images` - Image control
- `apps` - Game metadata
- `imageBytes` - Game header image data

**Creates**: ButtonContext with all components

### InitializeButtonListeners()
Attaches click handlers to game buttons.

**Action**: Opens ChooseGameWindow with selected game data

### TextBoxEventsInit()
Configures count input validation.

**Validation**: Clamps input to 1-5 range

### GoToFilter(object? sender, RoutedEventArgs e)
Opens filter configuration window.

## Style Classes

### Border.HeaderBorder
- Gray background
- Gradient border (start to end colors)
- Border thickness: 2
- Corner radius: 20

### TextBlock.HeaderText
- Font size: 24
- Bold weight
- White foreground
- Anime Ace font family

### Border.ContentBorder
- Semi-transparent black background (#66000000)
- Animated conic gradient border (3s rotation)
- Border thickness: 3
- Corner radius: 20

### Button.CloseButton
- Black background
- White foreground/border
- Border thickness: 2
- Corner radius: 20
- Animated border on hover (2s conic gradient)

### Border.GameBorder
- White border, thickness 3
- Gray background
- Margin: 10, Padding: 5

### Button.RandomButton
- Transparent background
- No border

### Image.GameImages
- Width: 80, Height: 80

### Button.GenerateButton
- Black background
- White border, thickness 2
- Height: 60

### Border.InputBorder
- White border, thickness 2
- Gray background
- Width: 60, Height: 60

### MaskedTextBox.CountInput
- Transparent background
- Font size: 36, bold
- White foreground and caret
- Centered content

## Usage Flow

1. User opens RollGame control
2. Optionally configures filters via "Filters" button
3. Sets game count (1-5) in input field
4. Checks filter checkbox if filtering desired
5. Clicks "Search" button
6. System generates random games matching criteria
7. User clicks game image to view details in ChooseGameWindow

## Integration Points

### Dependencies
- `IGenApp` - Game generation service
- `MainWindowFactory` - Grid layout management
- `FilterGameWindow` - Filter configuration
- `ChooseGameWindow` - Game details display
- `ErrorService` - Error handling
- `SteamService` - Image loading

### Navigation
- Implements `IUserControl` interface
- Uses `AddListener` for navigation callbacks
- Navigates to "Main" on close

## Error Handling

- Validates initialization before generation
- Prevents concurrent generation with `_isRolling` flag
- Shows error window for uninitialized state
- Handles null image bytes gracefully
- Throws exception for null bitmap conversion

## Disposal

Clears all references:
- Navigation callback
- Game generation service
- Error service
- App data dictionary
- Window factory

## Best Practices

1. Always check `_isRolling` before starting generation
2. Dispose properly when closing
3. Validate count input to prevent invalid states
4. Use iteration limit to prevent infinite loops
5. Handle async image loading with proper error checking
