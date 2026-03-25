using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.Scr.DI;
using GameRandom.Src;
using GameRandom.Src.Enums;

namespace GameRandom.Views;

public partial class MainWindowContent : MainWindowUserControlAbstract
{
    public MainWindowContent()
    {
        InitializeComponent();
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        
    }

    private void GoToRollContent(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Roll");
    }
    
    private void GoToTable(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Table");
    }

    private void GoToProfile(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Profile");
    }

    private void GoToRules(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Rules");
    }
}