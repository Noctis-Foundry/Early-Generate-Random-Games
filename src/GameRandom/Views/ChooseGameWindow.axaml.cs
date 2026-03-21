using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.CoreApp;
using GameRandom.Scr.DI;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class ChooseGameWindow : WindowAbstract
{
    [Inject] private ErrorService _errorService = null!;

    public ChooseGameWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = new ChooseGameViewModel();

        if (Design.IsDesignMode)
            return;

        Di.Container.ResolveField(out _errorService);
    }

    public void LoadData(AppSavedContext appSavedContext, byte[] imageBytes)
    {
        if (DataContext is ChooseGameViewModel viewModel)
        {
            viewModel.LoadGameInfo(appSavedContext, imageBytes);
        }
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (DataContext is ChooseGameViewModel viewModel)
            viewModel.Dispose();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is ChooseGameViewModel viewModel)
            viewModel.Dispose();
    }

    private void ToSteamStorePage(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ChooseGameViewModel viewModel)
            viewModel.ShowSteamStore();
    }

    private async void ChooseGame(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not ChooseGameViewModel viewModel) return;

            bool isAdd = await viewModel.ChooseGame();

            if (!isAdd)
                _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Failed to add game to database, try again", ErrorType = ErrorEnum.Message});
            else
                _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Game added to database", ErrorType = ErrorEnum.Message});
        }
        catch (Exception exception)
        {
            throw new Exception("Failed to add game progress to database: " + exception.Message);
        }
    }
}