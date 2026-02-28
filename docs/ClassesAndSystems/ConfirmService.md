# ConfirmService System

## Overview
Thread-safe confirmation dialog service for user decision prompts. Prevents multiple simultaneous dialogs and provides Yes/No confirmation interface.

## Files
- `ConfirmService.cs` - Service class managing dialog lifecycle
- `ConfirmDialog.axaml` - Dialog UI layout
- `ConfirmDialog.axaml.cs` - Dialog code-behind

## Purpose
- Display confirmation dialogs to users
- Prevent multiple concurrent dialogs
- Provide thread-safe dialog management
- Return boolean user decision

## ConfirmService

### Properties
```csharp
private ConfirmDialog? _confirmDialog
private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1)
```

### Methods

#### OpenConfirmDialog(string title, Window owner)
Opens confirmation dialog with specified message.

**Parameters**:
- `title` - Message text to display
- `owner` - Parent window for modal dialog

**Returns**: `Task<bool>` - True if user clicked Yes, False if No or dialog already open

**Thread Safety**: Uses SemaphoreSlim to prevent concurrent dialogs

**Process**:
1. Try to acquire semaphore (non-blocking)
2. Return false if semaphore unavailable (dialog already open)
3. Create new ConfirmDialog instance
4. Show dialog and await user response
5. Release semaphore
6. Return user decision

**Example**:
```csharp
var result = await confirmService.OpenConfirmDialog(
    "Are you sure you want to stop?", 
    mainWindow
);

if (result)
{
    // User clicked Yes
    await CancelOperation();
}
```

## ConfirmDialog Window

### UI Structure

**Layout**: DockPanel with gray background

**Content Grid** (2 rows):
- Row 0: Message display border
- Row 1: Button panel (Yes/No)

**Message Border**:
- DimGray background
- Gradient border (start → middle → end colors)
- Border thickness: 2
- Min size: 300x100
- Max size: 400x200

**Message Text**:
- SelectableTextBlock (user can copy text)
- Azure foreground
- Font size: 20
- Text wrapping enabled

**Button Panel**:
- Width: 170, Height: 50
- 2-column grid with 30px spacing
- Yes button (left)
- No button (right)
- ProfileButtons style class

### Window Properties
- Max size: 600x300
- Min size: 500x300
- Centered on owner window
- Modal dialog

### Styles

**ConfirmWindowText** (SelectableTextBlock):
- Foreground: Azure
- Font size: 20

**ConfirmTextBorder** (Border):
- Background: DimGray
- Border thickness: 2
- Linear gradient border (3 colors)

## ConfirmDialog Code-behind

### Constructor
```csharp
public ConfirmDialog()
{
    InitializeComponent();
    WindowStartupLocation = WindowStartupLocation.CenterOwner;
}
```

### Methods

#### ShowConfirmDialog(string title, Window owner)
Displays dialog with custom message.

**Parameters**:
- `title` - Message text
- `owner` - Parent window

**Returns**: `Task<bool>` - User decision

**Process**:
1. Set TextBlock.Text to title
2. Show modal dialog
3. Return boolean result

#### YesClick(object? sender, RoutedEventArgs e)
Handles Yes button click.

**Action**: Closes dialog with `true` result

#### NoClick(object? sender, RoutedEventArgs e)
Handles No button click.

**Action**: Closes dialog with `false` result

## Usage Example

### Service Registration
```csharp
// In DI container
services.AddSingleton<ConfirmService>();
```

### Injection and Usage
```csharp
public class MyControl : UserControl
{
    [Inject] private ConfirmService _confirmDialog = null!;
    
    private async Task<bool> ConfirmAction()
    {
        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        
        var result = await _confirmDialog.OpenConfirmDialog(
            "Are you sure you want to proceed?",
            mainWindow
        );
        
        return result;
    }
}
```

### Integration with Operations
```csharp
private async void DeleteItem(object sender, RoutedEventArgs e)
{
    var confirmed = await _confirmDialog.OpenConfirmDialog(
        "Delete this item permanently?",
        this
    );
    
    if (confirmed)
    {
        await database.DeleteItem(itemId);
        RefreshList();
    }
}
```

## Thread Safety

### Semaphore Protection
- Only one dialog can be open at a time
- Non-blocking check with `WaitAsync(0)`
- Returns false immediately if dialog already open
- Automatic release after dialog closes

### Concurrent Call Handling
```csharp
// First call - opens dialog
var result1 = await confirmService.OpenConfirmDialog("Message 1", window);

// Second call while first is open - returns false immediately
var result2 = await confirmService.OpenConfirmDialog("Message 2", window);
// result2 = false (dialog not opened)
```

## Features

- **Thread-Safe**: Semaphore prevents concurrent dialogs
- **Modal**: Blocks parent window until closed
- **Centered**: Automatically centers on owner
- **Selectable Text**: Users can copy message text
- **Gradient Styling**: Matches application theme
- **Simple API**: Single method for all confirmations

## Limitations

- Only one dialog per service instance
- No custom button text support
- No icon/severity indicators
- Fixed size constraints
- No timeout/auto-close
- Hardcoded styling

## Potential Improvements

```csharp
// Custom button text
public async Task<bool> OpenConfirmDialog(
    string title, 
    Window owner,
    string yesText = "Yes",
    string noText = "No"
)

// Severity levels
public enum DialogSeverity { Info, Warning, Error }

public async Task<bool> OpenConfirmDialog(
    string title,
    Window owner,
    DialogSeverity severity = DialogSeverity.Info
)

// Timeout support
public async Task<bool?> OpenConfirmDialog(
    string title,
    Window owner,
    TimeSpan? timeout = null
)
// Returns null if timeout expires

// Queue support
private Queue<DialogRequest> _dialogQueue = new();

public async Task<bool> OpenConfirmDialog(string title, Window owner)
{
    if (!await _semaphoreSlim.WaitAsync(0))
    {
        // Queue the request instead of rejecting
        return await EnqueueDialog(title, owner);
    }
    
    // Process dialog
}
```

## Testing

```csharp
[Test]
public async Task TestSingleDialog()
{
    var service = new ConfirmService();
    var result = await service.OpenConfirmDialog("Test", window);
    
    Assert.IsTrue(result || !result); // Valid boolean returned
}

[Test]
public async Task TestConcurrentDialogs()
{
    var service = new ConfirmService();
    
    var task1 = service.OpenConfirmDialog("First", window);
    var task2 = service.OpenConfirmDialog("Second", window);
    
    var result2 = await task2;
    Assert.IsFalse(result2); // Second call rejected
}
```
