using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.CoreApp;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;

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

        TaskRunner.RunWithDispatcherAsync(async () => await viewModel.ChooseGame());
    }
}