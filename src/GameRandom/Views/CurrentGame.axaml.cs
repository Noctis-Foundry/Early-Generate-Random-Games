using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class CurrentGame : WindowAbstract
{
    public CurrentGame()
    {
        InitializeComponent();
        DataContext = new CurrentGameStatusViewModel();

        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
    
    public override void Open(Window? parent = null)
    {
        base.Open(parent);
        
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (DataContext is CurrentGameStatusViewModel viewModel)
        {
            var uiBlocks = new GameStatusInfo(GameName, StartDate, TodayDate,
                TimeSpent, EndDate, GameImage);

            Dispatcher.UIThread.InvokeAsync(async () => await viewModel.LoadInfo(uiBlocks));
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is CurrentGameStatusViewModel viewModel)
        {
            viewModel.ClearingContent();
        }
        
        base.OnClosing(e);
    }

    private void ShowSteamStore(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CurrentGameStatusViewModel vm) return;

        var url = SteamService.Instance.AppSteamPage(vm.UserGame.AppId);

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private async void FinishedGame(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CurrentGameStatusViewModel vm) return;

        await vm.FinishingGame();
    }
}