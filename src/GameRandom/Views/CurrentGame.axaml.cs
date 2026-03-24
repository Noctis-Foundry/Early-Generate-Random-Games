using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class CurrentGame : WindowAbstract
{
    [Inject] private SteamService? _steamService;
    
    public CurrentGame()
    {
        InitializeComponent();
        DataContext = new CurrentGameStatusViewModel();

        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Di.Container.ResolveField(out _steamService);

        if (_steamService is null)
            throw new NullReferenceException("Failed to inject steam service from di");
    }
    
    public override void Open(Window? parent = null)
    {
        base.Open(parent);
        
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (DataContext is CurrentGameStatusViewModel viewModel)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await viewModel.LoadInfo();
            });
        }
    }

    public void ProfileClosed()
    {
        if (DataContext is CurrentGameStatusViewModel vm)
            vm.ClearingContent();
    }

    private void ShowSteamStore(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CurrentGameStatusViewModel vm) return;

        var url = _steamService.AppSteamPage(vm.UserGame.AppId);

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