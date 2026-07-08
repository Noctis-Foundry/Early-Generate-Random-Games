using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DbContext;
using GameRandom.Scripts;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public sealed partial class AdminConfirmWindow : WindowBase<AdminConfirmViewModel>
{
    private Task _currentThread;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    
    public AdminConfirmWindow()
    {
        InitializeComponent();
        InitializeViewModel();
        InitializeDiContainer();
    }
    
    public void LoadData(FinishedGames elementData)
    {
        Show();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (DataContext is AdminConfirmViewModel vm)
                await vm.UpdateElementData(elementData, _cts.Token);
        });
    }
    private void ConfirmGame(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AdminConfirmViewModel vm) return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            bool isSuccessfully = await vm.ConfirmGame();
            
            if (isSuccessfully)
                Close();
        });
    }
    private void RejectGame(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AdminConfirmViewModel vm) return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            bool isSuccessfully = await vm.RejectGame();
            
            if (isSuccessfully)
                Close();
        });
    }

    public override void Dispose() //Base dispose clearing data from view model
    {
        _cts.Cancel();
        _cts.Dispose();
        
        base.Dispose();
    }
}