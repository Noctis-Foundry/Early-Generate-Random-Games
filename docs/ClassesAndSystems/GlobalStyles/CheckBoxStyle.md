# CheckBox Global Style

## Overview
Comprehensive styling for CheckBox controls with animated gradient borders and state-specific appearances.

## File
`GlobalStyles/CheckBoxStyle.axaml`

## Resources

### RectangularMargin
- Value: `10,0,0,0`
- Applied to internal border rectangle

### LightStaticLinear
- Type: LinearGradientBrush
- Direction: Horizontal (0%, 0% → 100%, 0%)
- Colors: GradientColorStart → GradientColorMiddle → GradientColorEnd
- Used for static borders

## Default Style (CheckBox)

### Properties
- Background: `Gray`
- Foreground: `White`
- CornerRadius: `20`
- FontFamily: `{StaticResource 'Anime Ace'}`
- FontSize: `12`
- MinWidth: `100`
- HorizontalContentAlignment: `Center`
- ClipToBounds: `True`
- BorderThickness: `1.5`
- BorderBrush: `{StaticResource LightStaticLinear}`

## State Styles

### Unchecked (CheckBox:unchecked)
- NormalRectangle border: LightStaticLinear gradient
- Margin: RectangularMargin

### Pointer Over (CheckBox:pointerover)
- Foreground: `White`
- Background: `Gray`
- Animated rotating conic gradient border (3s duration, infinite)
- Animation: 0° → 360° rotation
- Gradient colors: Start → Middle → End
- Applied to both PART_Border and NormalRectangle

### Checked (CheckBox:checked)
- Background: `Gray`
- Foreground: `White`
- BorderThickness: `1`
- BorderBrush: LightStaticLinear
- NormalRectangle: LightStaticLinear border with margin
- CheckGlyph: Right-aligned

## Animations

### Hover Animation
- Duration: 3 seconds
- Iteration: Infinite
- Type: Conic gradient rotation
- Start: 0° angle
- End: 360° angle
- Applied to border on hover

## Usage

### Basic CheckBox
```xml
<CheckBox Content="Option 1"/>
<!-- Gray background, white text, gradient border, rounded corners -->
```

### Checked State
```xml
<CheckBox IsChecked="True" Content="Selected"/>
<!-- Shows checkmark on right, gradient border -->
```

### Custom Content
```xml
<CheckBox Content="Filter" Width="100"/>
<!-- Centered content, minimum 100px width -->
```

## Visual Features
- Rounded corners (radius 20)
- Gradient borders (static and animated)
- Smooth hover animation
- Centered content alignment
- Custom Anime Ace font
