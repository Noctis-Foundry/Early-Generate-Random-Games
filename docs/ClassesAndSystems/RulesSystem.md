# Rules System

## Overview
UserControl displaying game challenge rules with scrollable content. Shows 10 rules with pricing structure for game completion rewards.

## Purpose
- Display challenge rules and guidelines
- Show pricing structure for completed games
- Provide scrollable rule list
- Support bilingual content (planned)

## Layout

### Window Properties
- Background: Black
- 2-row grid layout
- Row 0: Header and close button (Auto)
- Row 1: Scrollable content (fills remaining)

### Header Section
- Title: "CHALLENGE RULES"
- Font size: 24px, Bold, White
- Underlined text decoration
- Close button (top-right)

### Close Button
- Content: "✕ CLOSE"
- Black background, white text/border
- Size: 108x30
- Border: 2px white

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
- Border: 1px white
- Background: #111111 (very dark gray)
- Padding: 15px
- White text, size 14px
- Text wrapping enabled

## Code-behind (Rules.axaml.cs)

### Fields
- `_isEnglish` (bool) - Language toggle state
- `_onShowContent` (Action<string>) - Navigation callback

### Methods

**AddListener(Action<string> onChangeContent)**
Registers navigation callback.

**Close(object? sender, RoutedEventArgs e)**
Navigates to "Main" page.

**ToggleLanguage(object? sender, RoutedEventArgs e)** (Implemented but not connected)
- Toggles between English/Russian
- Updates button text
- Calls UpdateTextToEnglish() or UpdateTextToRussian()

**UpdateTextToEnglish()**
Updates all TextBlock content to English translations.

**UpdateTextToRussian()**
Updates all TextBlock content to Russian translations.

### Language Support

**English Translations**:
- All rules translated
- Pricing in rubles maintained
- Button labels in English

**Russian Translations**:
- Original rule text
- Native language content
- Cyrillic characters

## Features

- **Scrollable Content**: Handles long rule list
- **Bilingual Support**: English/Russian (code ready, UI not connected)
- **Clear Structure**: Numbered rules with borders
- **Pricing Transparency**: Detailed reward structure
- **Navigation Integration**: IUserControl compatible

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

## Limitations

- Language toggle button not in UI
- No x:Name attributes for TextBlocks (language switching incomplete)
- Hardcoded rule text
- No rule editing capability
- Fixed pricing structure
- No localization framework
- Close button requires x:Name for language switching

## Potential Improvements

```csharp
// Add language toggle button to XAML
<Button Content="ENGLISH" 
        Click="ToggleLanguage"
        x:Name="LanguageButton"
        HorizontalAlignment="Left"/>

// Add x:Name to all TextBlocks
<TextBlock x:Name="TitleText" Text="CHALLENGE RULES"/>
<TextBlock x:Name="Rule1Text" Text="..."/>

// Use resource files for localization
<TextBlock Text="{Binding Source={StaticResource Strings}, Path=Rule1}"/>

// Dynamic pricing
public class RulesViewModel
{
    public ObservableCollection<PricingTier> Pricing { get; set; }
    public ObservableCollection<Rule> Rules { get; set; }
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

## Testing

```csharp
[Test]
public void TestLanguageToggle()
{
    var rules = new Rules();
    rules.ToggleLanguage(null, null);
    
    Assert.IsTrue(rules._isEnglish);
    
    rules.ToggleLanguage(null, null);
    Assert.IsFalse(rules._isEnglish);
}
```
