using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class StatisticControl : UserControl
{
    private GamesTableWindow? _gameTableWindow;
    
    public StatisticControl()
    {
        InitializeComponent();
        DataContext = new StatisticViewModel();

        if (Design.IsDesignMode)
            return;
        
        _gameTableWindow = new GamesTableWindow();
    }

    public void Open()
    {
        if (DataContext is StatisticViewModel statisticViewModel)
            Dispatcher.UIThread.InvokeAsync(async () => await statisticViewModel.LoadStatisticAsync(StatisticCardGrid));
    }

    public void Close()
    {
        if (DataContext is StatisticViewModel statisticViewModel)
            statisticViewModel.Dispose();
    }

    private void OpenTable(object? sender, RoutedEventArgs e)
    {
        if (_gameTableWindow is null)
        {
            Console.WriteLine("Statistic Control: error for open table window. Element is null");
            return;
        }
        
        _gameTableWindow.Open();
    }
}