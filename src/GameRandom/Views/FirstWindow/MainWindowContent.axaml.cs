using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.Scripts.UserControls;
using GameRandom.ViewModels;
using GameRandom.ViewModels.MainWindowSystem.Enums;

namespace GameRandom.Views;

public partial class MainWindowContent : MainWindowUserControlAbstract<MainUserControlViewModel>
{
    public MainWindowContent()
    {
        InitializeComponent();
        DataContext = new MainUserControlViewModel();
    }

    public override void CloseUserControl(object? sender, RoutedEventArgs e)
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