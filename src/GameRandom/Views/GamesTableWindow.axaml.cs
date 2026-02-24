using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class GamesTableWindow : WindowAbstract
{
    public GamesTableWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        DataContext = new StatisticGameTableViewModel();
    }

    public override void Open(Window? parent = null)
    {
        base.Open(parent);
        
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
            Dispatcher.UIThread.InvokeAsync(async () => await statisticGameTableViewModel.LoadData(e => e.PlayerId == User.GetInstance().GetUserInfo().SteamId));
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        if (DataContext is StatisticGameTableViewModel statisticGameTableViewModel)
            statisticGameTableViewModel.UnloadTable();
    }
}