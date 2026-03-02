# Rules System

## Overview
UserControl displaying game challenge rules with bilingual support (English/Russian). Features scrollable content with 10 rules and pricing structure for game completion rewards. Uses JSON-based localization system with external styling.

## Purpose
- Display challenge rules and guidelines
- Show pricing structure for completed games
- Provide scrollable rule list
- Support bilingual content (English/Russian)
- Maintain consistent visual styling

## Architecture

### Files
- **Rules.axaml** - UI layout with style classes
- **Rules.axaml.cs** - Code-behind with localization logic
- **RulesStyle.axaml** - External style definitions
- **Localization/Rules.en.json** - English translations
- **Localization/Rules.ru.json** - Russian translations

## Layout

### Window Properties
- Background: #2A2A2A (dark gray)
- 2-row grid layout
- Row 0: Header with title and buttons (Auto)
- Row 1: Scrollable content (fills remaining)

### Header Section
- **Title**: Dynamic text from localization
  - Font: 24px, Bold, White, Underlined
  - Center aligned
- **Language Toggle Button**: Switches between EN/RU
  - Size: 108x35
  - Top-right alignment
- **Close Button**: Returns to main view
  - Content: "✕ CLOSE"
  - Size: 108x35
  - Top-right alignment

### Button Styling
- Background: #3A3A3A (gray)
- Foreground: White
- Border: 2px #CCCCCC (light gray)
- Hover: #4A4A4A background, white border
- Corner radius: 4px

### Rules Content

Scrollable area with 10 bordered rule cards:

**Rule 1**: Initial roll count (3 rolls, variable)

**Rule 2**: Bonus roll for fast completion (check Cubiq.ru)

**Rule 3**: Maximum difficulty requirement, hardcore mode bonus

**Rule 4**: Difficulty reduction option (costs 1 roll)

**Rule 5**: Multiplayer game requirements (5 hours, shared rewards)

**Rule 6**: Payment structure with pricing tiers:
- 1-5 hours: 150 rubles
- 5-10 hours: 250 rubles
- 10-25 hours: 500 rubles
- 25+ hours: 1000 rubles
- 100% completion: Full game price

**Rule 7**: Bank system (50,000 rubles, penalties/rewards)

**Rule 8**: Drop penalty (minus 1 roll, random year)

**Rule 9**: Speedrun penalty (minus 1 roll, no rule 2 bonus)

**Rule 10**: Reroll policy for completed games

### Rule Card Styling
- Border: 1px #CCCCCC (light gray)
- Background: #3A3A3A (gray)
- Padding: 15px
- Corner radius: 4px
- White text, size 14px
- Text wrapping enabled

## Code-behind (Rules.axaml.cs)

### Fields
- `_isEnglish` (bool) - Language toggle state (default: false/Russian)
- `_currentLocalization` (Dictionary<string, string>) - Current language strings
- `_localizationPath` (string) - Path to localization folder
- `TitleText`, `Rule1Text`...`Rule10Text` (TextBlock) - UI element references
- `PricesTitleText`, `Price1Text`...`Price5Text` (TextBlock) - Pricing UI references
- `LanguageButton` (Button) - Language toggle button reference

### Constructor
```csharp
public Rules()
{
    InitializeComponent();
    _localizationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Localization");
    LoadLocalization("en");
}
```
Initializes component, sets localization path, loads English by default.

### Methods

**Close(object? sender, RoutedEventArgs e)**
Closes the rules window and returns to main view.

**LoadLocalization(string language)**
- Constructs path to JSON file: `Rules.{language}.json`
- Validates file existence (throws FileNotFoundException)
- Reads and deserializes JSON to dictionary
- Calls UpdateText() to apply translations

**ToggleLanguage(object? sender, RoutedEventArgs e)**
- Toggles `_isEnglish` flag
- Loads "en" or "ru" localization
- Updates all UI text automatically

**UpdateText()**
Applies current localization to all UI elements:
- Title and all 10 rules
- Pricing title and 5 price entries
- Language button content

## Localization System

### JSON Structure
```json
{
  "Title": "CHALLENGE RULES",
  "Rule1": "1. The initial number...",
  "Rule2": "2. If a player completes...",
  ...
  "PricesTitle": "Prices for completing games:",
  "Price1": "• Game from 1-5 hours: 150 rubles",
  ...
  "LanguageButton": "РУССКИЙ"
}
```

### Supported Languages
- **English** (Rules.en.json) - Default language
- **Russian** (Rules.ru.json) - Alternative language

### File Location
`Assets/Localization/Rules.{language}.json`

## Styling System (RulesStyle.axaml)

### Style Classes

**RulesGrid**
- Background: #2A2A2A

**TitleText**
- Font: 24px, Bold, White
- Center aligned, underlined
- Margin: 0,20,0,10

**RulesButton**
- Background: #3A3A3A
- Foreground: White
- Border: 2px #CCCCCC
- Size: 108x35
- Corner radius: 4px
- Hover: #4A4A4A background, white border

**RuleBorder**
- Border: 1px #CCCCCC
- Background: #3A3A3A
- Padding: 15px
- Corner radius: 4px

**RuleText**
- Foreground: White
- Font: 14px
- Text wrapping enabled

**PriceText**
- Foreground: White
- Font: 13px

## Features

- **Bilingual Support**: Full English/Russian localization
- **JSON-based Translations**: Easy to add new languages
- **External Styling**: Centralized visual design
- **Scrollable Content**: Handles long rule list
- **Interactive Language Toggle**: Real-time switching
- **File Validation**: Checks localization file existence
- **Clean Architecture**: Separation of concerns (UI/Logic/Style/Data)

## Usage Example

```csharp
var factory = new UserControlFactory();
var rulesControl = factory.CreateUserControl<Rules>(pageName =>
{
    NavigateToPage(pageName);
});

rulesControl.Open();
contentArea.Content = rulesControl;
```

## Integration

### Opening from Menu
```csharp
// MainWindow menu item
<MenuItem Header="Rules" Command="{Binding OpenRules}"/>

// ViewModel
public void OpenRules()
{
    Navigate("Rules");
}
```

## Adding New Languages

1. Create new JSON file: `Rules.{code}.json`
2. Copy structure from existing file
3. Translate all values
4. Update ToggleLanguage logic if needed:

```csharp
private void ToggleLanguage(object? sender, RoutedEventArgs e)
{
    _currentLanguage = (_currentLanguage + 1) % 3; // For 3 languages
    var languages = new[] { "en", "ru", "de" };
    LoadLocalization(languages[_currentLanguage]);
}
```

## Error Handling

- **FileNotFoundException**: Thrown if localization file missing
- **InvalidOperationException**: Thrown if JSON deserialization fails
- Both exceptions provide clear error messages

## Testing

```csharp
[Test]
public void TestLanguageToggle()
{
    var rules = new Rules();
    Assert.IsFalse(rules._isEnglish); // Starts with Russian
    
    rules.ToggleLanguage(null, null);
    Assert.IsTrue(rules._isEnglish); // Switches to English
    
    rules.ToggleLanguage(null, null);
    Assert.IsFalse(rules._isEnglish); // Back to Russian
}

[Test]
public void TestLocalizationLoading()
{
    var rules = new Rules();
    rules.LoadLocalization("en");
    
    Assert.IsNotNull(rules._currentLocalization);
    Assert.IsTrue(rules._currentLocalization.ContainsKey("Title"));
}
```

## Rule Summary

1. **Base Rolls**: 3 (variable)
2. **Speed Bonus**: +1 roll for fast completion
3. **Difficulty**: Max difficulty required, hardcore +1 roll
4. **Difficulty Reduction**: -1 roll penalty
5. **Multiplayer**: 5 hours minimum, shared rewards
6. **Pricing**: Tiered by completion time
7. **Bank System**: 50K rubles, penalties/rewards
8. **Drop Penalty**: -1 roll, random year
9. **Speedrun**: -1 roll, no speed bonus
10. **Rerolls**: Allowed for completed games

## Improvements Over Previous Version

- ✅ Language toggle fully implemented and connected
- ✅ All TextBlocks have x:Name attributes
- ✅ JSON-based localization (no hardcoded text)
- ✅ External styling system
- ✅ File validation
- ✅ Clean separation of concerns
- ✅ Gray color scheme with hover effects
- ✅ Direct field access (no FindControl in UpdateText)
