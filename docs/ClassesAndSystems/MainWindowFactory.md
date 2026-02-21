# MainWindowFactory

## Overview
Factory service for dynamically creating and managing Avalonia UI grid layouts with buttons and images. Simplifies UI generation for game display grids.

## Purpose
- Dynamic grid column generation
- Batch button/image creation
- Consistent UI element styling
- Grid layout management

## Methods

### ChangeGrid(int countImage, Grid grid)
Reconfigures grid column definitions.

**Parameters**:
- `countImage` - Number of columns to create
- `grid` - Target grid control

**Process**:
1. Clear existing column definitions
2. Add new auto-sized columns

**Note**: TODO comment suggests changing loop to `i = 0; i < countImage`

**Example**:
```csharp
factory.ChangeGrid(5, gameGrid);
// Creates 5 auto-sized columns
```

### CreateButtonInGrid(int countImage, Grid grid)
Creates buttons with embedded images in grid.

**Parameters**:
- `countImage` - Number of button-image pairs to create
- `grid` - Target grid control

**Returns**: `(List<Button>, List<Image>)` - Created buttons and images

**Process**:
1. Clear existing grid children
2. For each index:
   - Create Image with default icon
   - Apply "GameImages" style class
   - Create Button with image as content
   - Apply "RandomButton" style class
   - Set grid column position
   - Add to grid

**Naming Convention**:
- Images: `AppImage{index}`
- Buttons: `AppButton{index}`

**Default Image**: `./Assets/avalonia-logo.ico`

**Example**:
```csharp
var (buttons, images) = factory.CreateButtonInGrid(3, gameGrid);

// Customize
for (int i = 0; i < buttons.Count; i++)
{
    buttons[i].Click += OnGameClick;
    images[i].Source = gameBitmaps[i];
}
```

### CreateImagesInGrid(int countImage, Grid grid)
Creates standalone images in grid without buttons.

**Parameters**:
- `countImage` - Number of images to create
- `grid` - Target grid control

**Returns**: `List<Image>` - Created images

**Process**:
1. Clear existing grid children
2. Create images using CreateImageInGrid
3. Return image list

**Example**:
```csharp
var images = factory.CreateImagesInGrid(4, avatarGrid);

foreach (var img in images)
{
    img.Source = avatarBitmap;
}
```

### CreateImageInGrid(Grid grid, int imageColumn)
Creates single image at specified grid column.

**Parameters**:
- `grid` - Target grid control
- `imageColumn` - Column index for image

**Returns**: `Image` - Created image

**Process**:
1. Create Image with default icon
2. Set grid column position
3. Add to grid
4. Return image

**Example**:
```csharp
var avatarImage = factory.CreateImageInGrid(topPanel, 0);
avatarImage.Source = userAvatar;
```

## ButtonContext Class

### Purpose
Container for button-image pairs.

### Properties

**Button** (Button) - Button control

**ButtonImage** (Image) - Image inside button

### Constructor
```csharp
ButtonContext(Button button, Image buttonImage)
```

### Usage
```csharp
var context = new ButtonContext(button, image);
context.Button.Click += OnClick;
context.ButtonImage.Source = newBitmap;
```

## Usage Patterns

### Game Selection Grid
```csharp
var factory = new MainWindowFactory();

// Setup grid
factory.ChangeGrid(5, gameGrid);

// Create interactive game buttons
var (buttons, images) = factory.CreateButtonInGrid(5, gameGrid);

// Load game data
for (int i = 0; i < games.Count; i++)
{
    images[i].Source = await LoadGameImage(games[i]);
    buttons[i].Click += (s, e) => SelectGame(games[i]);
}
```

### Avatar Display Panel
```csharp
// Setup columns
factory.ChangeGrid(lobbyMembers.Count, avatarPanel);

// Create avatar images
var avatars = factory.CreateImagesInGrid(lobbyMembers.Count, avatarPanel);

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
    var (buttons, images) = factory.CreateButtonInGrid(games.Count, gameGrid);
    
    // Populate with game data
    PopulateGames(buttons, images, games);
}
```

## Style Classes

### GameImages
Applied to all images created in buttons.

**Purpose**: Consistent image styling

### RandomButton
Applied to all buttons created.

**Purpose**: Consistent button styling

**Example CSS**:
```css
.GameImages {
    Width: 200;
    Height: 300;
}

.RandomButton {
    Background: Transparent;
    BorderThickness: 0;
}
```

## Features

- **Dynamic Layout**: Adjusts to any number of elements
- **Auto-Sizing**: Columns automatically size to content
- **Batch Creation**: Efficient multi-element generation
- **Style Integration**: Automatic CSS class application
- **Flexible**: Supports buttons with images or standalone images

## Best Practices

1. **Call ChangeGrid First**: Set columns before adding elements
2. **Store References**: Keep button/image lists for updates
3. **Clear Before Recreate**: Methods clear grid automatically
4. **Use Style Classes**: Define appearance in CSS, not code
5. **Index Consistency**: Match data array indices to element indices

## Limitations

- Clears all grid children on each call
- Only supports horizontal layouts (columns)
- Fixed naming convention for elements
- No built-in event handler attachment
- No element recycling/pooling
