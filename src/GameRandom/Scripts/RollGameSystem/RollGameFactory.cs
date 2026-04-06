using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Service;
using GameRandom.Src.Factory;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.Views;

namespace GameRandom.Src.RollGameSystem;

public class RollGameFactory : IDisposable
{
    [Inject] private MainWindowFactory _mainWindowFactory = null!;
    [Inject] private SteamService _steamService = null!;
    
    private const int IterationDelayMilliseconds = 200;
    
    private List<RollButtonsInfo> _buttonsInfo = new();
    
    private GifImage _loadGif = null!;
    
    public RollGameFactory()
    {
        if (Design.IsDesignMode)
            return;
        
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_mainWindowFactory is null)
            throw new NullReferenceException(nameof(_mainWindowFactory));
        if (_steamService is null)
            throw new NullReferenceException(nameof(_steamService));
    }

    public void CreateLoadGif(Grid grid)
    {
        _loadGif = new GifImage();
        _loadGif = _mainWindowFactory.CreateAnimatedImage(grid);
    }
    
    public async Task GenerateUi(RollGameViewModel viewModel, Grid grid, int countApp)
    {
        SetupGrid(countApp, grid);
        
        for (int i = 0; i < viewModel.AppInfo.Count; i++)
        {
            var gridElements = _mainWindowFactory.CreateButtonInGrid(grid, i);
            InitDictionaryWithComponents(gridElements, viewModel.AppInfo[i]);

            await Task.Delay(IterationDelayMilliseconds);
        }
        
        grid.Children.Remove(_loadGif);
        _loadGif = null!;
    }

    private void SetupGrid(int countGames, Grid grid)
    {
        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();

        for (int i = 0; i < countGames; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
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


    public void Dispose()
    {
        _buttonsInfo.Clear();
        _buttonsInfo = null!;

        _steamService = null!;
        _mainWindowFactory = null!;
    }
}