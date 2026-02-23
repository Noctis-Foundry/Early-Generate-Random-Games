using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class CurrentGame : Window
{
    [Inject]
    private DatabaseService _databaseService;
    
    [Inject]
    private ErrorService _errorService;
    
    public CurrentGame()
    {
        InitializeComponent();
        DataContext = new CurrentGameStatusViewModel();

        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
    
    public void Open()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        Show();
        
        if (DataContext is CurrentGameStatusViewModel viewModel)
        {
            var uiBlocks = new GameStatusInfo(GameName, StartDate, TodayDate,
                TimeSpent, EndDate, GameImage);

            Dispatcher.UIThread.InvokeAsync(async () => await viewModel.LoadInfo(uiBlocks));
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Очистка данных
        _databaseService = null;
        _errorService = null;
        
        if (DataContext is CurrentGameStatusViewModel viewModel)
        {
            viewModel.CloseCurrentGameWindow();
        }
    }

    private void ShowSteamStore(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void CheckStatus(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }


    private void FinishedGame(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}