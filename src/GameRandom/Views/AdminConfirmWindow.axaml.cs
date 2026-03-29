using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public sealed partial class AdminConfirmWindow : WindowBase<AdminConfirmViewModel>
{
    public AdminConfirmWindow()
    {
        InitializeComponent();
        InitializeViewModel();
    }
    public async void LoadData(FinishedGames elementData)
    {
        Show();
        
        if (DataContext is AdminConfirmViewModel vm)
            await vm.UpdateElementData(elementData);
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
}