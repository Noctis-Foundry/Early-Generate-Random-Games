using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class GamesTableWindow : Window
{
    public GamesTableWindow()
    {
        InitializeComponent();
        DataContext = new StatisticGameTableViewModel();
    }

    public void Open()
    {
        Show();
        
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
            Dispatcher.UIThread.InvokeAsync(async () => statisticGameTableViewModel.LoadTable());
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
            statisticGameTableViewModel.UnloadTable();
    }
}