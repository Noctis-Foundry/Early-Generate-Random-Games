# MenuItem Global Style

## Overview
Styling for menu items with hover effects.

## File
`GlobalStyles/MenuItem.axaml`

## Default Style (MenuItem)

### Properties
- Foreground: `WhiteSmoke`
- FontFamily: `{DynamicResource 'Anime Ace'}`

## Hover Style (MenuItem:pointerover)

### ContentPresenter
- Foreground: `DarkSeaGreen`

## Usage

### Menu with Items
```xml
<Menu Background="Gray">
    <MenuItem Header="Lobby System"/>
    <MenuItem Header="Rules"/>
</Menu>
<!-- WhiteSmoke text, changes to DarkSeaGreen on hover -->
```

## Visual Features
- Light text on dark backgrounds
- Color change on hover for feedback
- Custom Anime Ace font
- Consistent with application theme
