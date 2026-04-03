using System;
using GameRandom.DependenceInjectSystem;
using System.Diagnostics;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using Avalonia.Interactivity;
using GameRandom.DependenceInjectSystem;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.DependenceInjectSystem;

namespace GameRandom.Views;

public sealed partial class CurrentGame : WindowBase<CurrentGameStatusViewModel>
{
    [Inject] private SteamService _steamService;
    
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

        if (vm.UserGame is null)
        {
            ShowMessage("Game is empty");
            return;
        }

        var url = _steamService.AppSteamPage(vm.UserGame.AppId);

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private void FinishedGame(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CurrentGameStatusViewModel vm)
            return;

        if (vm.IsEmpty)
        {
            ShowMessage("Game is empty");
            return;
        }
        
        TaskRunner.RunWithDispatcherAsync(async () => await vm.FinishingGame());
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
        
        base.Dispose();
    }
}