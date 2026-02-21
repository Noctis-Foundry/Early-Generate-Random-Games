# Colors

## Overview
Static utility class providing predefined Avalonia color constants for consistent UI theming.

## Purpose
- Centralized color definitions
- Consistent color usage across UI
- Easy color reference

## Properties

### LightSlateGray
**RGB**: (119, 136, 153)

**Hex**: #778899

**Usage**: Neutral gray-blue tone

### Teal
**RGB**: (0, 128, 128)

**Hex**: #008080

**Usage**: Accent color, highlights

## Usage Examples

### Button Styling
```csharp
button.Background = new SolidColorBrush(Colors.Teal);
button.Foreground = new SolidColorBrush(Colors.LightSlateGray);
```

### Border Colors
```csharp
border.BorderBrush = new SolidColorBrush(Colors.LightSlateGray);
```

### XAML Binding
```xml
<Button Background="{x:Static local:Colors.Teal}" />
```

### Dynamic Color Application
```csharp
public void ApplyTheme(bool isDarkMode)
{
    panel.Background = isDarkMode 
        ? new SolidColorBrush(Colors.LightSlateGray)
        : new SolidColorBrush(Colors.Teal);
}
```

## Features

- **Static Access**: No instantiation required
- **Type Safety**: Avalonia Color type
- **Reusability**: Single source of truth for colors

## Best Practices

1. **Use for Consistency**: Reference these colors instead of hardcoding RGB values
2. **Extend as Needed**: Add new colors to this class for project-wide use
3. **Document Usage**: Comment where each color is intended to be used

## Potential Improvements

```csharp
public static class Colors
{
    // Primary colors
    public static Color Primary = Color.FromRgb(0, 128, 128);
    public static Color Secondary = Color.FromRgb(119, 136, 153);
    
    // State colors
    public static Color Success = Color.FromRgb(40, 167, 69);
    public static Color Warning = Color.FromRgb(255, 193, 7);
    public static Color Error = Color.FromRgb(220, 53, 69);
    public static Color Info = Color.FromRgb(23, 162, 184);
    
    // Neutral colors
    public static Color Background = Color.FromRgb(248, 249, 250);
    public static Color Text = Color.FromRgb(33, 37, 41);
    
    // Helper methods
    public static Color WithOpacity(Color color, double opacity)
    {
        return Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);
    }
}
```
