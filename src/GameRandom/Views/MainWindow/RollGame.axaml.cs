using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using GameRandom.CoreApp;
using GameRandom.Service;
using GameRandom.SteamSDK;

namespace GameRandom.Views;

public partial class RollGame : UserControl
{
    private readonly Dictionary<Button, AppSavedContext?> _appData = new ();
    private readonly Dictionary<int, Image> _imageData = new Dictionary<int, Image>();
    
    private readonly Random _random = new();
    
    private readonly IGenApp _generateRandomApps;
    private readonly MainWindowFactory _mainWindowFactory;

    private Action<string> _onShowContent;
    private bool _isRolling = false;
    
    public RollGame()
    {
        InitializeComponent();

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

        int lastYear = 0;
        
        while (apps.Count < countGames)
        {
            int year = _random.Next(2000, 2025);

            if (lastYear == year)
            {
                continue;
            }

            lastYear = year;
            
            var game = _generateRandomApps.GetRandomGame(year);

            if (!apps.Contains(game) && game != null)
            {
                apps.Add(game);
            }
        }
        
        countGames = apps.Count;

        if (countGames == 0)
        {
            Console.WriteLine("No games found");
            _isRolling = false;
            return;
        }
        
        _mainWindowFactory.ChangeGrid(countGames, GamesGrid);
        (List<Button> buttons, List<Image> images) = _mainWindowFactory.CreateButtonInGrid(countGames, GamesGrid);
        
        AddListenerOnButtonClick(buttons);
        InitDictionaryWithComponents(buttons, images);
        
        for (int i = 0; i < apps.Count; i++)
        {
            if (apps[i] is null)
                continue;
            
            Bitmap? bitmap = await SteamService.Instance.GetImage(apps[i].HeaderImage);

            if (bitmap is null)
                continue;
            
            if (_appData.ContainsKey(buttons[i]))
            {
                _appData[buttons[i]] = apps[i];
            }

            _imageData[i].Source = bitmap;
        }

        _isRolling = false;
    }

    private void AddListenerOnButtonClick(List<Button> buttons)
    {
        foreach (var button in buttons)
        {
            button.Click += (sender, e) =>
            {
                var btn = sender as Button;
                TakeGame(btn, e);
            };
        }
    }

    private void InitDictionaryWithComponents(List<Button> buttons, List<Image> images)
    {
        _appData.Clear();
        _imageData.Clear();
        
        foreach (var button in buttons)
        {
            _appData[button] = null;
        }

        for (var i = 0; i < images.Count; i++)
        {
            _imageData.Add(i, images[i]);
        }
    }
    
    private void TakeGame(object? sender, RoutedEventArgs e)
    {
        Button? button = (Button?)sender;

        if (button != null)
        {
            if (_appData.TryGetValue(button, out AppSavedContext? appData))
            {
                if (appData != null)
                {
                    Console.WriteLine($"App name {appData.AppName}");
                }
                else
                {
                    Console.WriteLine($"App error");
                }
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