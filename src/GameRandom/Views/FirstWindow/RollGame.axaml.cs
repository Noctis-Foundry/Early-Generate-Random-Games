using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using GameRandom.CoreApp;
using GameRandom.Scr.DI;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class RollGame : UserControl, IUserControl, IDisposable //TODO Refactoring classes and extract logic to view model
{
    [Inject] private ErrorService _errorService = null!;
    
    private const int MinYear = 2003;
    private const int MaxYear = 2026;

    private Dictionary<ButtonContext, AppSavedContext?> _appData = new();
    private ChooseGameWindow _chooseGameWindow = new();
    private FilterGameWindow _filterGameWindow = new();

    private IGenApp _generateRandomApps;
    private MainWindowFactory _mainWindowFactory;

    private Action<string>? _onShowContent;
    private bool _isRolling = false;

    public RollGame()
    {
        InitializeComponent();
        DataContext = new RollGameViewModel();

        if (Design.IsDesignMode)
            return;

        TextBoxEventsInit();

        _generateRandomApps = new GenerateRandomApps();
        _mainWindowFactory = new MainWindowFactory();
    }

    public void AddListener(Action<string> _onChangeContent) => _onShowContent = _onChangeContent;

    private async void GenerateGames(object sender, RoutedEventArgs e)
    {
        if (!_generateRandomApps.IsInitialized || _isRolling)
        {
            _errorService.ShowErrorWindow("Generating random games not initialized.", ErrorEnum.Error);
            return;
        }

        _isRolling = true;

        int countGames = int.Parse(CountApp.Text ?? "1");

        _appData.Clear();
        _mainWindowFactory.ChangeGrid(countGames, GamesGrid);

        int iterCount = 0;

        List<AppSavedContext> savedGames = new();

        while (_appData.Count < countGames && iterCount < 1000)
        {
            var year = FilterCheckBox.IsChecked == true ? _filterGameWindow.GetYear() : Random.Shared.Next(MinYear, MaxYear);
            var gameInfo = _generateRandomApps.GetRandomGame(year);

            if (gameInfo is null || savedGames.Contains(gameInfo))
                continue;

            if (FilterCheckBox.IsChecked == true)
            {
                if (!_filterGameWindow.CheckFilters(gameInfo))
                    continue;
            }
            
            var imageBytes = await SteamService.Instance.GetImageBytes(gameInfo.HeaderImage);

            if (imageBytes is null)
                continue;

            var gridElements = _mainWindowFactory.CreateButtonInGrid(GamesGrid, iterCount);
            InitDictionaryWithComponents(gridElements.Button, gridElements.Image, gameInfo, imageBytes);

            savedGames.Add(gameInfo);
            iterCount++;
        }

        InitializeButtonListeners();
        _isRolling = false;
    }
    
    public void Close(object? sender, RoutedEventArgs e)
    {
        //TODO
        _onShowContent?.Invoke("Main");
        Dispose();
    }

    public void Open()
    {
        //TODO
    }

    private void InitDictionaryWithComponents(Button buttons, Image images, AppSavedContext apps,
        byte[] imageBytes)
    {
        ButtonContext buttonContext = new ButtonContext(buttons, images, imageBytes);

        Bitmap? bitmap = SteamService.Instance.GetImageSyncFromBytes(imageBytes);

        if (bitmap is null)
            throw new NullReferenceException("Failed to get bitmap from bytes");

        buttonContext.ButtonImage.Source = bitmap;

        if (!_appData.TryAdd(buttonContext, apps))
        {
            _errorService.ShowErrorWindow(
                $"Dictionary contains duplicated app button with hash code {Equals(buttonContext)}",
                ErrorEnum.Error);
        }
    }

    private void InitializeButtonListeners()
    {
        foreach (var item in _appData)
        {
            var button = item.Key.Button;
            
            if (item.Value != null)
                button.Command = new RelayCommand(() =>
                {
                    _chooseGameWindow.Open();
                    _chooseGameWindow.LoadData(item.Value, item.Key.ImageBytes);
                });
            else
                throw new Exception("Not find game");
        }
    }

    private void TextBoxEventsInit()
    {
        CountApp.PropertyChanged += (sender, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                var text = CountApp.Text;

                if (int.TryParse(text, out var count))
                {
                    var num = Math.Clamp(count, 1, 5);
                    if (num.ToString() != CountApp.Text)
                        CountApp.Text = num.ToString();
                }
            }
        };
    }

    public void Dispose()
    {
        _onShowContent = null;
        _generateRandomApps = null;
        _errorService = null!;
        _appData = null;
        _mainWindowFactory = null;
    }

    private void GoToFilter(object? sender, RoutedEventArgs e)
    {
        _filterGameWindow.Open();
    }
}