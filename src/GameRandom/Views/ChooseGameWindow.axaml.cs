using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scripts;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.ChooseGameSystem;

namespace GameRandom.Views;

public sealed partial class ChooseGameWindow : WindowBase<ChooseGameViewModel>
{
    public ChooseGameWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        InitializeViewModel();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        InitializeDiContainer();
        InitializeProcessingHandler();
    }

    public void LoadData(AppSavedContext appSavedContext, byte[] imageBytes)
    {
        if (DataContext is ChooseGameViewModel viewModel)
        {
            viewModel.LoadGameInfo(appSavedContext, imageBytes);
        }
    }

    private void ToSteamStorePage(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ChooseGameViewModel viewModel)
            viewModel.ShowSteamStore();
    }

    private void ChooseGame(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ChooseGameViewModel viewModel) return;

        Dispatcher.UIThread.InvokeAsync(async () => await viewModel.ChooseGame());
    }
}