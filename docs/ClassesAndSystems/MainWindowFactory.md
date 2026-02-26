# MainWindowFactory

## Overview
Factory service for dynamically creating and managing Avalonia UI grid layouts with buttons and images.

## Purpose
- Dynamic grid column generation
- Button/image pair creation
- Consistent UI element styling
- Grid layout management

## Methods

### ChangeGrid(int countImage, Grid grid)
Reconfigures grid column definitions and clears children.

**Parameters**:
- `countImage` - Number of columns to create
- `grid` - Target grid control

**Process**:
1. Clear existing column definitions
2. Clear all grid children
3. Add new auto-sized columns

**Note**: Loop starts at 1 (TODO: change to 0-based)

**Example**:
```csharp
factory.ChangeGrid(5, gameGrid);
// Creates 5 auto-sized columns
```

### CreateButtonInGrid(Grid grid, int countGame)
Creates single button with embedded image in grid.

**Parameters**:
- `grid` - Target grid control
- `countGame` - Column index for button placement

**Returns**: `GridElements` - Container with button and image

**Process**:
1. Create Image with default icon
2. Apply "GameImages" style class
3. Create Button with image as content
4. Apply "RandomButton" style class
5. Set grid column position
6. Add button to grid
7. Return GridElements

**Naming Convention**:
- Images: `AppImage{countGame}`
- Buttons: `AppButton{countGame}`

**Default Image**: `Assets/avalonia-logo.ico`

**Example**:
```csharp
var elements = factory.CreateButtonInGrid(gameGrid, 0);
elements.Button.Click += OnGameClick;
elements.Image.Source = gameBitmap;
```

### CreateImageInGrid(Grid grid, int count)
Creates bordered images in grid columns.

**Parameters**:
- `grid` - Target grid control
- `count` - Number of images to create

**Returns**: `List<Image>` - Created images

**Process**:
1. Clear existing column definitions
2. Create star-sized columns
3. For each image:
   - Create Image with default icon
   - Wrap in Border (height 30-40, corner radius 10)
   - Set grid column position
   - Add to grid
4. Return image list

**Border Properties**:
- Height: 30 (min 30, max 40)
- ClipToBounds: true
- CornerRadius: 10

**Example**:
```csharp
var avatars = factory.CreateImageInGrid(avatarPanel, 4);
for (int i = 0; i < avatars.Count; i++)
{
    avatars[i].Source = await GetAvatar(members[i]);
}
```

## Helper Classes

### ButtonContext
Container for button-image-data association.

**Properties**:
- `Button` - Button control
- `ButtonImage` - Image inside button
- `ImageBytes` - Raw image data

**Constructor**:
```csharp
ButtonContext(Button button, Image buttonImage, byte[] imageBytes)
```

**Usage**:
```csharp
var context = new ButtonContext(button, image, imageData);
context.Button.Click += OnClick;
context.ButtonImage.Source = newBitmap;
```

### GridElements
Container for button-image pairs.

**Properties**:
- `Button` - Button control
- `Image` - Image control

**Constructor**:
```csharp
GridElements(Button button, Image image)
```

**Usage**:
```csharp
var elements = new GridElements(button, image);
elements.Button.Command = clickCommand;
elements.Image.Source = bitmap;
```

## Usage Patterns

### Game Selection Grid
```csharp
var factory = new MainWindowFactory();

// Setup grid
factory.ChangeGrid(5, gameGrid);

// Create game buttons
for (int i = 0; i < games.Count; i++)
{
    var elements = factory.CreateButtonInGrid(gameGrid, i);
    elements.Image.Source = await LoadGameImage(games[i]);
    elements.Button.Click += (s, e) => SelectGame(games[i]);
}
```

### Avatar Display Panel
```csharp
// Create avatar images with borders
var avatars = factory.CreateImageInGrid(avatarPanel, lobbyMembers.Count);

// Load avatars
for (int i = 0; i < lobbyMembers.Count; i++)
{
    avatars[i].Source = await GetAvatar(lobbyMembers[i]);
}
```

### Dynamic Resizing
```csharp
void UpdateGameDisplay(List<Game> games)
{
    factory.ChangeGrid(games.Count, gameGrid);
    
    for (int i = 0; i < games.Count; i++)
    {
        var elements = factory.CreateButtonInGrid(gameGrid, i);
        PopulateGame(elements, games[i]);
    }
}
```

## Style Classes

### GameImages
Applied to all images created in buttons.

**Purpose**: Consistent image sizing

### RandomButton
Applied to all buttons created.

**Purpose**: Consistent button styling

**Example AXAML**:
```xml
<Style Selector=".GameImages">
    <Setter Property="Width" Value="200"/>
    <Setter Property="Height" Value="300"/>
</Style>

<Style Selector=".RandomButton">
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="BorderThickness" Value="0"/>
</Style>
```

## Features

- **Dynamic Layout**: Adjusts to any number of elements
- **Auto-Sizing**: Columns automatically size to content
- **Individual Creation**: Create elements one at a time
- **Style Integration**: Automatic CSS class application
- **Flexible**: Supports buttons with images or standalone bordered images

## Best Practices

1. **Call ChangeGrid First**: Set columns before adding elements
2. **Store References**: Keep element references for updates
3. **Index Management**: Use consistent indexing for data-element mapping
4. **Use Style Classes**: Define appearance in AXAML, not code
5. **Async Image Loading**: Load images asynchronously to prevent UI blocking

## Limitations

- ChangeGrid clears all grid children
- CreateButtonInGrid creates one element at a time
- Only supports horizontal layouts (columns)
- Fixed naming convention for elements
- No built-in event handler attachment
- Loop indexing starts at 1 (needs refactoring)
