# Global Style System

## Overview
Application-wide XAML style definitions for consistent typography and visual appearance across all UI components.

## Purpose
- Define default styles for common controls
- Ensure consistent font family usage
- Set global color scheme
- Centralize visual theming

## Styles

### TextBlock (Global)
Default style applied to all TextBlock elements.

**Properties**:
- **FontFamily**: `Rye-Font` (DynamicResource)
- **Foreground**: `#1a1c1c` (Dark gray/black)

**Usage**: Automatically applied to all text blocks

**Example**:
```xml
<TextBlock Text="Hello World"/>
<!-- Automatically uses Rye-Font and #1a1c1c color -->
```

### Button (Global)
Default style applied to all Button elements.

**Properties**:
- **FontFamily**: `Rye-Font` (DynamicResource)
- **FontSize**: `16`
- **Foreground**: `#1a1c1c` (Dark gray/black)
- **Background**: `LightGray`

**Commented Code**: ImageBrush background option available for future use

**Usage**: Automatically applied to all buttons

**Example**:
```xml
<Button Content="Click Me"/>
<!-- Automatically uses Rye-Font, size 16, dark text on light gray -->
```

## Font Resource

### Rye-Font
Custom font family referenced as DynamicResource.

**Reference**: `{DynamicResource 'Rye-Font'}`

**Note**: Font must be defined in App.axaml or resource dictionary

**Expected Definition**:
```xml
<Application.Resources>
    <FontFamily x:Key="Rye-Font">avares://GameRandom/Assets/Fonts/Rye-Regular.ttf#Rye</FontFamily>
</Application.Resources>
```

## Color Scheme

### Primary Text Color
**Hex**: `#1a1c1c`

**RGB**: (26, 28, 28)

**Description**: Very dark gray, almost black

**Usage**: Text and foreground elements

### Primary Background Color
**Name**: `LightGray`

**Description**: Avalonia built-in light gray

**Usage**: Button backgrounds

## Design Preview

Includes preview configuration for design-time rendering:
```xml
<Design.PreviewWith>
    <Border Padding="20">
        <Button Content="Ar0cka"></Button>
    </Border>
</Design.PreviewWith>
```

Shows button with "Ar0cka" text in 20px padded border.

## Future Enhancements

### Image Background (Commented)
Template for image-based button backgrounds:
```xml
<Setter Property="Background">
    <Setter.Value>
        <ImageBrush Source="Path to background"></ImageBrush>
    </Setter.Value>
</Setter>
```

**To Enable**:
1. Uncomment code block
2. Replace "Path to background" with actual image path
3. Remove or comment LightGray background setter

## Usage in Application

### App.axaml Integration
```xml
<Application.Styles>
    <StyleInclude Source="avares://GameRandom/Styles/GlobalStyle.axaml"/>
</Application.Styles>
```

### Override Global Styles
```xml
<!-- Override for specific button -->
<Button Content="Special" 
        FontSize="20" 
        Background="Blue"/>

<!-- Override for specific text -->
<TextBlock Text="Custom" 
           FontFamily="Arial" 
           Foreground="Red"/>
```

### Extend Global Styles
```xml
<!-- Add to global styles without overriding -->
<Style Selector="Button.Primary">
    <Setter Property="Background" Value="Blue"/>
    <Setter Property="Foreground" Value="White"/>
</Style>

<Button Classes="Primary" Content="Primary Action"/>
```

## Best Practices

1. **Consistent Typography**: All text uses Rye-Font by default
2. **Override When Needed**: Use local styles for exceptions
3. **Use Classes**: Create style classes for variations
4. **Test Font Loading**: Ensure Rye-Font resource exists
5. **Color Consistency**: Use defined colors throughout app

## Potential Improvements

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Color Resources -->
    <Color x:Key="PrimaryText">#1a1c1c</Color>
    <Color x:Key="SecondaryText">#666666</Color>
    <Color x:Key="PrimaryBackground">#F0F0F0</Color>
    <Color x:Key="AccentColor">#008080</Color>
    
    <!-- Font Resources -->
    <FontFamily x:Key="PrimaryFont">Rye</FontFamily>
    <FontFamily x:Key="SecondaryFont">Arial</FontFamily>
    
    <!-- Global TextBlock -->
    <Style Selector="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource PrimaryFont}"/>
        <Setter Property="Foreground" Value="{StaticResource PrimaryText}"/>
    </Style>
    
    <!-- Global Button -->
    <Style Selector="Button">
        <Setter Property="FontFamily" Value="{StaticResource PrimaryFont}"/>
        <Setter Property="FontSize" Value="16"/>
        <Setter Property="Foreground" Value="{StaticResource PrimaryText}"/>
        <Setter Property="Background" Value="{StaticResource PrimaryBackground}"/>
        <Setter Property="Padding" Value="12,6"/>
        <Setter Property="CornerRadius" Value="4"/>
    </Style>
    
    <!-- Button Variants -->
    <Style Selector="Button.Primary">
        <Setter Property="Background" Value="{StaticResource AccentColor}"/>
        <Setter Property="Foreground" Value="White"/>
    </Style>
    
    <Style Selector="Button.Secondary">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderBrush" Value="{StaticResource AccentColor}"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>
    
    <!-- Heading Styles -->
    <Style Selector="TextBlock.Heading1">
        <Setter Property="FontSize" Value="32"/>
        <Setter Property="FontWeight" Value="Bold"/>
    </Style>
    
    <Style Selector="TextBlock.Heading2">
        <Setter Property="FontSize" Value="24"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
    </Style>
</Styles>
```

## Features

- **Global Consistency**: All controls share common styling
- **Easy Maintenance**: Single source for style changes
- **DynamicResource**: Font can be changed at runtime
- **Extensible**: Easy to add new global styles
- **Override Friendly**: Local styles take precedence

## Limitations

- Only styles TextBlock and Button
- No dark theme support
- Font resource must exist elsewhere
- Limited color palette
- No responsive sizing
- Commented code needs cleanup

## Integration Example

```csharp
// App.axaml.cs
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Global styles automatically applied
        var window = new MainWindow();
        window.Show();
        
        base.OnFrameworkInitializationCompleted();
    }
}
```

## Testing Styles

```xml
<!-- Test window for style verification -->
<Window xmlns="https://github.com/avaloniaui">
    <StackPanel Spacing="10" Margin="20">
        <TextBlock Text="Default TextBlock"/>
        <Button Content="Default Button"/>
        
        <TextBlock Text="Custom Color" Foreground="Red"/>
        <Button Content="Custom Background" Background="Blue"/>
    </StackPanel>
</Window>
```
