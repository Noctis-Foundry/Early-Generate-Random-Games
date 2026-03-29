using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using Avalonia.Labs.Gif;
using CommunityToolkit.Mvvm.Input;
using GameRandom.CoreApp;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.Factory;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public partial class RollGame : MainWindowUserControlAbstract
{
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private ConfirmService? _confirmDialog = null!;
    [Inject] private SteamService? _steamService;
    
    private FilterGameWindow? _filterGameWindow = new();

    private List<RollButtonsInfo> _buttonsInfo = new();
    private GifImage? _loadGif;

    private const int DefaultCountApp = 1;
    private const int IterationDelayMilliseconds = 500;
    private const int maxCountGames = 4;
    
    private SemaphoreSlim _rollSemaphore = new(1, 1);

    /// <summary>
    /// Initializes the RollGame control and its dependencies.
    /// </summary>
    public RollGame()
    {
        InitializeComponent();
        DataContext = new RollGameViewModel(new GenerateRandomApps());

        if (Design.IsDesignMode)
            return;

        TextBoxEventsInit();

        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_errorService is null || _confirmDialog is null)
            throw new NullReferenceException();

        if (_steamService is null)
            throw new NullReferenceException(nameof(_steamService));
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke("Main");
        
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
    private async void GenerateGame(object? sender, RoutedEventArgs e)
    {
        if (!await _rollSemaphore.WaitAsync(0))
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Wait for the generation to complete", ErrorType = ErrorEnum.Error});
            return;
        }

        try
        {
            if (DataContext is not RollGameViewModel viewModel) return;
            int countGames = int.TryParse(CountApp.Text, out int count) ? count : DefaultCountApp;
            
            SetupGrid(countGames);
            
            var filters = _filterGameWindow?.GetFilters();

            await viewModel.GenerateGames(countGames, filters);
            
            await GenerateUi();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
        finally
        {
            _rollSemaphore.Release();
        }
    }

    private async Task GenerateUi()
    {
        if (Di.Container.GetInstance<MainWindowFactory>() is not MainWindowFactory mainWindowFactory) return;
        if (DataContext is not RollGameViewModel viewModel) return;

        for (int i = 0; i < viewModel.AppInfo.Count; i++)
        {
            var gridElements = mainWindowFactory.CreateButtonInGrid(GamesGrid, i);
            InitDictionaryWithComponents(gridElements, viewModel.AppInfo[i]);

            await Task.Delay(IterationDelayMilliseconds);
        }

        if (_loadGif != null)
        {
            GamesGrid.Children.Remove(_loadGif);
            _loadGif = null;
        }
    }

    private void SetupGrid(int countGames = DefaultCountApp)
    {
        GamesGrid.Children.Clear();
        GamesGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < countGames; i++)
        {
            GamesGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        if (Di.Container.GetInstance<MainWindowFactory>() is MainWindowFactory mainWindowFactory)
        {
            _loadGif = mainWindowFactory.CreateAnimatedImage(GamesGrid);
        }
    }
    
    /// <summary>
    /// Associates game data with UI components and loads game image.
    /// </summary>
    private void InitDictionaryWithComponents(GridElements gridElements, AppInfo app)
    {
        Bitmap? bitmap = _steamService.GetImageSyncFromBytes(app.ImageBytes);

        if (bitmap is null)
            throw new NullReferenceException("Failed to get bitmap from bytes");

        gridElements.Image.Source = bitmap;

        RelayCommand appCommand = new RelayCommand(() =>
        {
            var chooseGameWindow = new ChooseGameWindow();
            chooseGameWindow.Show();
            chooseGameWindow.LoadData(app.AppData, app.ImageBytes);
        });

        gridElements.Button.Command = appCommand;

        _buttonsInfo.Add(new RollButtonsInfo(gridElements.Button, gridElements.Image, appCommand));
    }
    
    public override void Dispose()
    {
        _filterGameWindow?.Close();
        
        _rollSemaphore?.Dispose();
        _buttonsInfo.Clear();
        
        if (DataContext is RollGameViewModel viewModel)
            viewModel.Dispose();

        _errorService = null;
        _confirmDialog = null;
        _steamService = null;
    }
}