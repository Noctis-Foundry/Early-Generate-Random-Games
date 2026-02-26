# Button Global Style

## Overview
Global styling for all Button controls in the application.

## File
`GlobalStyles/Button.axaml`

## Properties

### FontFamily
- Value: `{DynamicResource Rye-Font}`
- Custom font for consistent button typography

### FontSize
- Value: `16`
- Standard button text size

### Foreground
- Value: `#1a1c1c`
- Dark gray text color

### Background
- Value: `LightGray`
- Default button background

## Notes
- Commented out ImageBrush option for background images
- Applied to all Button controls globally
- Can be overridden by specific button classes

## Usage
Automatically applied to all buttons:
```xml
<Button Content="Click Me"/>
<!-- Inherits Rye-Font, size 16, dark gray text, light gray background -->
```
