# ErrorService

## Overview
Service for displaying modal error dialogs with queuing support and global exception handling. Implements IError interface and extends Register for DI integration.

## Purpose
- Display error messages to users
- Queue multiple errors for sequential display
- Catch unhandled exceptions globally
- Thread-safe UI error handling

## Architecture

**Pattern**: Queue-based modal dialog manager

**Threading**: Uses Dispatcher for UI thread safety

## Fields

**_ownerWindow** (Window) - Parent window for modal dialogs

**_errorWindow** (ErrorWindow) - Reusable error dialog instance

**_queue** (Queue<ErrorStruct>) - Error message queue

**isActiveWindow** (bool) - Tracks if error window is currently displayed

## Constructor

### ErrorService()
Initializes service and sets up global exception handlers.

**Process**:
1. Call GlobalExceptionHandler()
2. Register unhandled exception handlers

## Methods

### Init<T1>(T1 arg1)
Initializes service with owner window.

**Parameters**:
- `arg1` - Owner window (must be Window type)

**Process**:
1. Cast and store owner window
2. Create ErrorWindow instance
3. Subscribe to Closed event

**Example**:
```csharp
var errorService = new ErrorService();
errorService.Init(mainWindow);
```

### ShowErrorWindow(string message, ErrorEnum errorType)
Displays error dialog or queues if one is already active.

**Parameters**:
- `message` - Error message text
- `errorType` - Error severity (Message, Warning, Error)

**Behavior**:
- If window active: Enqueue error
- If window inactive: Show immediately

**Process**:
1. Check if window is active
2. If active, enqueue and return
3. Set active flag
4. Update error window content
5. Show modal dialog on UI thread

**Example**:
```csharp
errorService.ShowErrorWindow("Connection failed", ErrorEnum.Error);
errorService.ShowErrorWindow("Settings saved", ErrorEnum.Message);
```

### GlobalExceptionHandler() (private)
Registers global exception handlers for unhandled errors.

**Handlers**:

**AppDomain.UnhandledException**
- Catches unhandled exceptions in main thread
- Displays error dialog on UI thread

**TaskScheduler.UnobservedTaskException**
- Catches unhandled async exceptions
- Marks exception as observed
- Displays error dialog

**Example Caught Exceptions**:
```csharp
// Unhandled exception
throw new Exception("Critical error");

// Unobserved task exception
Task.Run(() => throw new Exception("Async error"));
```

### ClosedWindow() (private)
Handles error window close event.

**Process**:
1. Reset active flag
2. Try to show next queued error

### SaveInvokeOnUI(Action action) (private)
Safely invokes action on UI thread.

**Parameters**:
- `action` - Action to execute on UI thread

**Usage**: Ensures exception handlers can update UI

### TryGoNext() (private)
Processes next error in queue if available.

**Process**:
1. Check if queue has items and window is inactive
2. Dequeue next error
3. Show error window

## IError Interface

### Purpose
Contract for error display services.

### Method
```csharp
void ShowErrorWindow(string message, ErrorEnum errorType);
```

## ErrorStruct

### Purpose
Data structure for queued error messages.

### Fields

**ErrorType** (ErrorEnum) - Error severity

**ErrorMessage** (string) - Error text

### Usage
```csharp
var error = new ErrorStruct 
{ 
    ErrorType = ErrorEnum.Warning, 
    ErrorMessage = "Low disk space" 
};
```

## ErrorEnum

Expected values (defined elsewhere):
- `Message` - Informational
- `Warning` - Non-critical issue
- `Error` - Critical error

## Usage Examples

### Basic Error Display
```csharp
var errorService = new ErrorService();
errorService.Init(mainWindow);

errorService.ShowErrorWindow("Database connection failed", ErrorEnum.Error);
```

### Multiple Errors (Queuing)
```csharp
// First error shows immediately
errorService.ShowErrorWindow("Error 1", ErrorEnum.Error);

// Second error queued
errorService.ShowErrorWindow("Error 2", ErrorEnum.Warning);

// Third error queued
errorService.ShowErrorWindow("Error 3", ErrorEnum.Message);

// Errors display sequentially as user closes each dialog
```

### Integration with Try-Catch
```csharp
try
{
    await RiskyOperation();
}
catch (Exception ex)
{
    errorService.ShowErrorWindow(ex.Message, ErrorEnum.Error);
}
```

### Global Exception Handling
```csharp
// Automatically caught and displayed
public void SomeMethod()
{
    throw new InvalidOperationException("Something went wrong");
    // ErrorService automatically shows dialog
}
```

## Features

- **Queue Management**: Sequential error display
- **Global Exception Handling**: Catches unhandled exceptions
- **Thread Safety**: UI thread marshalling via Dispatcher
- **Modal Dialogs**: Blocks interaction until dismissed
- **Reusable Window**: Single ErrorWindow instance
- **DI Integration**: Extends Register for dependency injection

## Thread Safety

All error displays are marshalled to UI thread:
```csharp
// Safe from any thread
Task.Run(() => 
{
    errorService.ShowErrorWindow("Background error", ErrorEnum.Error);
});
```

## Best Practices

1. **Initialize Early**: Call Init() during application startup
2. **Use Appropriate Severity**: Match ErrorEnum to situation
3. **Keep Messages Clear**: User-friendly, actionable text
4. **Don't Overuse**: Avoid error fatigue with too many dialogs
5. **Log Errors**: Use Logger in addition to ErrorService

## Integration with DI

```csharp
var factory = new DiFactory();
factory.Create<ErrorService, Window>(new ErrorService(), mainWindow);

// Inject into services
public class DatabaseService
{
    [Inject] private readonly ErrorService _errorService = null!;
    
    public async Task Connect()
    {
        try
        {
            await ConnectToDatabase();
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorWindow($"Database error: {ex.Message}", ErrorEnum.Error);
        }
    }
}
```

## ErrorWindow

### Purpose
Modal dialog window for displaying error messages with severity-based styling.

### Layout
- Size: 500x150
- Background: Gray
- Centered content with 40px row spacing

### UI Elements

**ErrorLabel** (SelectableTextBlock)
- White text with text wrapping
- Displays error message
- Selectable for copying

**ErrorButton** (Button)
- Content: "Ok" or "Close app" (based on severity)
- Black background, white text
- Size: 100px width, centered
- Font size: 16px

### Methods

**ChangeTextOnModal(string text, ErrorEnum errorType)**
- Updates error message text
- Sets window title to error type
- Changes button text based on severity
- Stores current error type

**MessageBoxButtonAction(object? sender, RoutedEventArgs e)**
- Closes window on button click

**OnClosed()**
- Called when window closes
- If error type is Critical: Exits application (Environment.Exit(0))

### Features
- **Center Owner**: Opens centered on parent window
- **Critical Error Handling**: Closes app on critical errors
- **Selectable Text**: Users can copy error messages
- **Dynamic Button**: Changes based on error severity

### Usage
```csharp
var errorWindow = new ErrorWindow();
errorWindow.ChangeTextOnModal("Database connection failed", ErrorEnum.Error);
await errorWindow.ShowDialog(mainWindow);
```

## Limitations

- Single ErrorWindow instance (no parallel dialogs)
- Queue has no size limit (potential memory issue)
- No error deduplication
- No timeout or auto-dismiss
- Simple gray styling

## Potential Improvements

```csharp
public class ErrorService
{
    private const int MaxQueueSize = 10;
    private HashSet<string> _recentErrors = new();
    
    public void ShowErrorWindow(string message, ErrorEnum errorType, int timeoutMs = 0)
    {
        // Deduplicate
        if (_recentErrors.Contains(message))
            return;
        
        _recentErrors.Add(message);
        
        // Limit queue size
        if (_queue.Count >= MaxQueueSize)
        {
            Logger.Warning("Error queue full, dropping oldest error");
            _queue.Dequeue();
        }
        
        // Auto-dismiss for non-critical errors
        if (timeoutMs > 0 && errorType != ErrorEnum.Error)
        {
            Task.Delay(timeoutMs).ContinueWith(_ => _errorWindow.Close());
        }
        
        // Existing logic...
    }
}
```
