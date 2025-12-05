using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;

namespace GameRandom.Views;

public partial class MainWindowContent : UserControl
{
    private ContentControl _rollContent;
    private Action<string> _changeContent;
    
    public MainWindowContent()
    {
        InitializeComponent();
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;
    
    private void GoToRollContent(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Roll");
    }
    
    private void GoToTable(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Table");
    }

    private void GoToProfile(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Profile");
    }

    private void GoToRules(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Rules");
    }
}