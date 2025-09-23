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
    private readonly Action<string> _changeContent;
    
    public MainWindowContent(Action<string> changeContent)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        _changeContent = changeContent;
    }
    
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
}