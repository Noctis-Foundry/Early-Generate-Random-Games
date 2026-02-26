# TextBlock Global Style

## Overview
Global styling for all TextBlock controls in the application.

## File
`GlobalStyles/TextBlock.axaml`

## Properties

### FontFamily
- Value: `{DynamicResource 'Rye-Font'}`
- Custom Rye font for consistent typography

### Foreground
- Value: `#1a1c1c`
- Dark gray text color

## Usage

### Basic TextBlock
```xml
<TextBlock Text="Hello World"/>
<!-- Inherits Rye-Font and dark gray color -->
```

### With Custom Properties
```xml
<TextBlock Text="Title" FontSize="24" FontWeight="Bold"/>
<!-- Keeps Rye-Font and color, adds size and weight -->
```

## Notes
- Applied to all TextBlock controls globally
- Can be overridden by specific classes or inline styles
- Ensures consistent text appearance across application
