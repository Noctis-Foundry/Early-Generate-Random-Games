using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public sealed partial class CurrentGame : WindowBase<CurrentGameStatusViewModel>
{
    [Inject] private SteamService? _steamService;
    
    public CurrentGame()
    {
        InitializeComponent();
        InitializeViewModel();
        InitializeDiContainer();
        InitializeProcessingHandler();

        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public override void Show()
    {
        base.Show();

        if (DataContext is CurrentGameStatusViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await vm.LoadInfo());
        }
    }

    public void ProfileClosed()
    {
        Dispose();
        Close();
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
        if (DataContext is not CurrentGameStatusViewModel vm)
            return;

        if (vm.IsEmpty)
        {
            ShowMessage("Game is empty");
            return;
        }
        
        await TaskRunner.Run(async () => await vm.FinishingGame());
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_steamService is null)
            throw new NullReferenceException("Failed to inject steam service from di");
    }

    public override void Dispose()
    {
        _steamService = null;
        
        if (DataContext is CurrentGameStatusViewModel vm)
            vm.Dispose();
        
        base.Dispose();
    }
}