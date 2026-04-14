using System;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scripts.RollGameSystem.GenerateGames;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Src.RollGameSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Views;

public partial class RollGame : MainWindowUserControlAbstract
{
    [Inject] private IErrorService _errorService = null!;
    
    private FilterGameWindow? _filterGameWindow = new();

    private CancellationTokenSource _cts = new();

    private const int DefaultCountApp = 1;

    private const int maxCountGames = 4;
    
    private RollGameFactory _rollGameFactory = new RollGameFactory();
    private SemaphoreSlim _rollSemaphore = new(1, 1);

    /// <summary>
    /// Initializes the RollGame control and its dependencies.
    /// </summary>
    public RollGame()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        DataContext = new RollGameViewModel(new GenerateRandomApps());
        TextBoxEventsInit();

        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.MainWindow);
        
        Dispose();
    }

    /// <summary>
    /// Configures count input validation to clamp values between 1-5.
    /// </summary>
    private void TextBoxEventsInit()
    {
        CountApp.PropertyChanged += (sender, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                var text = CountApp.Text;

                if (int.TryParse(text, out var count))
                {
                    if (count >  maxCountGames)
                        CountApp.Text = maxCountGames.ToString();
                }
            }
        };
    }

    /// <summary>
    /// Opens the filter configuration window.
    /// </summary>
    private void GoToFilter(object? sender, RoutedEventArgs e)
    {
        _filterGameWindow = new FilterGameWindow();
        _filterGameWindow.Show();
    }

    /// <summary>
    /// Call generate game event from view model
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GenerateGame(object? sender, RoutedEventArgs e)
    {
        TaskRunner.RunWithDispatcherAsync(async () => await GenerateGameAsync());
    }

    private async Task GenerateGameAsync()
    {
        if (!await _rollSemaphore.WaitAsync(0))
        {
            _errorService.ShowWindow("Generation game is not empty");
            return;
        }
        
        if (DataContext is not RollGameViewModel viewModel) 
            throw new NullReferenceException(nameof(DataContext));
        
        int countGames = int.TryParse(CountApp.Text, out int count) ? count : DefaultCountApp;

        GamesGrid.Children.Clear();
        _rollGameFactory.CreateLoadGif(GamesGrid);
        
        var filters = _filterGameWindow?.GetFilters();

        await TaskRunner.Run(() => viewModel.GenerateGames(countGames, filters, _cts.Token));
        await TaskRunner.RunWithFinallyAction(() => _rollGameFactory.GenerateUi(viewModel, GamesGrid, countGames), () => _rollSemaphore.Release());
    }
    
    public override void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null!;
        
        _filterGameWindow?.Close();
        _filterGameWindow?.Dispose();
        
        _rollSemaphore?.Dispose();
        _rollGameFactory?.Dispose();
        _rollGameFactory = null!;
        
        if (DataContext is RollGameViewModel viewModel)
            viewModel.Dispose();

        _errorService = null!;
    }
}