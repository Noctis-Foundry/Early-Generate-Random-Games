# UserControlFactory

## Overview
Factory class for creating user control instances with navigation callback injection. Simplifies instantiation of UI controls implementing IUserControl interface.

## Purpose
- Creates user control instances with type safety
- Automatically injects navigation callback
- Ensures consistent initialization pattern

## Method

### CreateUserControl<TType>(Action<string> onNavigate)
Creates and initializes user control with navigation listener.

**Type Constraints**:
- `TType : IUserControl` - Must implement IUserControl interface
- `TType : new()` - Must have parameterless constructor

**Parameters**:
- `onNavigate` - Navigation callback invoked when control requests page change

**Returns**: `TType` - Initialized user control instance

**Process**:
1. Instantiate TType using default constructor
2. Inject navigation callback via AddListener
3. Return configured instance

## IUserControl Interface

### Methods

**AddListener(Action<string> onChangeContent)**
Registers navigation callback for page transitions.

**Open()**
Opens/displays the user control.

**Close(object? sender, RoutedEventArgs e)**
Closes/hides the user control.

## Usage Example

```csharp
var factory = new UserControlFactory();

// Create control with navigation
var menuControl = factory.CreateUserControl<MenuControl>(pageName =>
{
    Console.WriteLine($"Navigate to: {pageName}");
    NavigateToPage(pageName);
});

// Control is ready to use
menuControl.Open();
```

## Navigation Pattern

```csharp
public class MenuControl : UserControl, IUserControl
{
    private Action<string> _onNavigate;
    
    public void AddListener(Action<string> onChangeContent)
    {
        _onNavigate = onChangeContent;
    }
    
    private void OnButtonClick()
    {
        _onNavigate?.Invoke("SettingsPage");
    }
    
    public void Open() { /* Show UI */ }
    public void Close(object? sender, RoutedEventArgs e) { /* Hide UI */ }
}
```

## Benefits

- **Type Safety**: Generic constraint ensures correct interface implementation
- **Consistent Initialization**: All controls initialized with navigation callback
- **Simplified Creation**: Single method call replaces manual instantiation and setup
- **Decoupling**: Controls don't need to know navigation implementation details

## Design Pattern
Implements Factory Pattern with dependency injection for callback-based navigation system.
