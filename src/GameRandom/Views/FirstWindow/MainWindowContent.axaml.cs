using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Views;

public partial class MainWindowContent : MainWindowUserControlAbstract
{
    public MainWindowContent()
    {
        InitializeComponent();
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        //Empty
    }

    private void GoToRollContent(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.Roll);
    }
    
    private void GoToTable(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.GameTable);
    }

    private void GoToProfile(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.Profile);
    }
}