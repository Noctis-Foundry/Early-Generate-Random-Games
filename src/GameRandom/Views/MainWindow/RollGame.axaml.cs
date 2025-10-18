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
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class RollGame : UserControl
{
    private readonly Dictionary<ButtonContext, AppSavedContext?> _appData = new ();
    
    private readonly Random _random = new();
    
    private readonly IGenApp _generateRandomApps;
    private readonly MainWindowFactory _mainWindowFactory;

    private Action<string> _onShowContent;
    private bool _isRolling = false;

    private int _lastYear = 0;
    
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
            Console.WriteLine("Generating random games not initialized.");
            return;
        }

        _isRolling = true;
        
        int countGames = int.Parse(CountApp.Text ?? "1");
        
        List<AppSavedContext?> apps = new List<AppSavedContext?>();
        
        while (apps.Count < countGames)
        {
            int year = _random.Next(2000, 2025);

            if (_lastYear == year)
            {
                continue;
            }
            _lastYear = year;
            
            var game = _generateRandomApps.GetRandomGame(year);
            if (!apps.Contains(game) && game != null)
            {
                apps.Add(game);
            }
        }

        if (apps.Count <= 0)
        {
            Console.WriteLine("No games found");
            _isRolling = false;
            return;
        }
        
        _mainWindowFactory.ChangeGrid(apps.Count, GamesGrid);
        (List<Button> buttons, List<Image> images) = _mainWindowFactory.CreateButtonInGrid(apps.Count, GamesGrid);
        
        await InitDictionaryWithComponents(buttons, images, apps);
        InitializeButtonListeners();

        _isRolling = false;
    }
    
    private async Task InitDictionaryWithComponents(List<Button> buttons, List<Image> images, List<AppSavedContext?> apps)
    {
        for (int i = 0; i < apps.Count; i++)
        {
            ButtonContext buttonContext = new ButtonContext(buttons[i], images[i]);

            Bitmap imageBitmap = await SteamService.Instance.GetImage(apps[i].HeaderImage);
            buttonContext.ButtonImage.Source = imageBitmap;
            
            if (!_appData.TryAdd(buttonContext, apps[i]))
            {
                throw new Exception(
                    $"Dictionary contains duplicated app button with hash code {Equals(buttonContext)}");
            }
        }
    }

    private void InitializeButtonListeners()
    {
        foreach (var item in _appData)
        {
            var button = item.Key.Button;
            
            if (DataContext is RollGameViewModel vm)
            {
                if (item.Value != null)
                    button.Command = new RelayCommand(async () => await vm.ChooseGame(item.Value));
                else
                    throw new Exception("Not find game");
            }
            else
            {
                Console.WriteLine("Not find data context");
            }
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
    private void Close(Object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }
}