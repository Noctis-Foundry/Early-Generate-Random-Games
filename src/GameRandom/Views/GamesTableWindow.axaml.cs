using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.Scripts;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.TablesSystem;

namespace GameRandom.Views;

public sealed partial class GamesTableWindow : WindowBase<StatisticGameTableViewModel>
{
    public GamesTableWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeProcessingHandler();
    }

    public override void Show()
    {
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await statisticGameTableViewModel.LoadData());
        }
        
        base.Show();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
            statisticGameTableViewModel.Dispose();
    }
}