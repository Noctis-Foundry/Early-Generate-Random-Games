using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.Views;

public partial class Rules : WindowAbstract
{
    private bool _isEnglish = false;
    private Dictionary<string, string> _currentLocalization = new();
    private readonly string _localizationPath;
    
    public Rules()
    {
        InitializeComponent();
        _localizationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Localization");
        LoadLocalization("en");
    }
    
    private void Close(Object? sender, RoutedEventArgs e)
    {
        CloseWindow();
    }

    private void LoadLocalization(string language)
    {
        var path = Path.Combine(_localizationPath, $"Rules.{language}.json");

        if (!File.Exists(path))
        { 
            throw new FileNotFoundException($"File {path} does not exist"); 
        }
        
        var json = File.ReadAllText(path);
        
        _currentLocalization = JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
                               ?? throw new InvalidOperationException("Failed to deserialize localization file.");
        UpdateText();
    }

    private void ToggleLanguage(object? sender, RoutedEventArgs e)
    {
        _isEnglish = !_isEnglish;
        LoadLocalization(_isEnglish ? "en" : "ru");
    }
    private void UpdateText()
    {
        TitleText.Text = _currentLocalization["Title"];
        Rule1Text.Text = _currentLocalization["Rule1"];
        Rule2Text.Text = _currentLocalization["Rule2"];
        Rule3Text.Text = _currentLocalization["Rule3"];
        Rule4Text.Text = _currentLocalization["Rule4"];
        Rule5Text.Text = _currentLocalization["Rule5"];
        Rule6Text.Text = _currentLocalization["Rule6"];
        PricesTitleText.Text = _currentLocalization["PricesTitle"];
        Price1Text.Text = _currentLocalization["Price1"];
        Price2Text.Text = _currentLocalization["Price2"];
        Price3Text.Text = _currentLocalization["Price3"];
        Price4Text.Text = _currentLocalization["Price4"];
        Price5Text.Text = _currentLocalization["Price5"];
        Rule7Text.Text = _currentLocalization["Rule7"];
        Rule8Text.Text = _currentLocalization["Rule8"];
        Rule9Text.Text = _currentLocalization["Rule9"];
        Rule10Text.Text = _currentLocalization["Rule10"];
        LanguageButton.Content = _currentLocalization["LanguageButton"];
    }
}
    