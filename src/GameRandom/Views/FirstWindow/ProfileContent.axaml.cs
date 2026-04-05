using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.ViewModels;
using GameRandom.ViewModels.AdminConfirmSystem;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : MainWindowUserControlAbstract
{
    private StatisticControl _statisticContent;
    private CurrentGame? _currentGame;

    public ProfileContent()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            StatisticContent.Content = new StatisticControl();
        }
        
        DataContext = new ProfileViewModel();
    }

    public override void Open()
    {
        _statisticContent = new StatisticControl();
        _statisticContent.Open();
        
        StatisticContent.Content = _statisticContent;
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Main");
        Dispose();
    }

    public override void Dispose()
    {
        _changeWindowAction = null;
        ProfileImage.Source = null;
        
        if (DataContext is IDisposable d)
        {
            d.Dispose();
        }
        
        DataContext = null;
        
        _currentGame?.ProfileClosed();
        _statisticContent.Dispose();
    }

    private void OpenActivityGame(object? sender, RoutedEventArgs e)
    {
        _currentGame = new CurrentGame();
        _currentGame.Show();
    }
}