using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using GameRandom.CoreApp;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;
using Avalonia.Animation;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private readonly IGenApp _generateRandomApps = new GenerateRandomApps();
    private readonly Dictionary<Button, AppSavedContext?> _appData = new ();
    private readonly Dictionary<int, Image> _imageData = new Dictionary<int, Image>();
    private readonly MainWindowFactory _mainWindowFactory;
    
    private readonly Random _random = new();
    
    private const int MaxCountItem = 5;
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        _mainWindowFactory = new MainWindowFactory();
        TextBoxEventsInit();
        InitializePlayerProfile();
    }

    private void InitializePlayerProfile()
    {
        var clientID = SteamManager.Instance.GetSteamID();

        string accName = SteamFriends.GetPersonaName();
        
        int imageId = SteamFriends.GetLargeFriendAvatar(clientID);

        uint width, height;
        SteamUtils.GetImageSize(imageId, out width, out height);
        
        byte[] image = new byte[width * height * 4];
        SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));

        var bitmap = AvaloniaService.CreateBitmap(image, (int)width, (int)height);
        
        AvatarImage.Source = bitmap;
        AccName.Content = accName;
    }
    
    private async void GenerateGames(object sender, RoutedEventArgs e)
    {
        if (!_generateRandomApps.IsInitialized)
        {
            Console.WriteLine("Generating random games not initialized.");
            return;
        }
        
        int countGames = int.Parse(CountApp.Text ?? "1");
        
        List<AppSavedContext?> apps = new List<AppSavedContext?>();

        while (apps.Count < countGames)
        {
            var game = _generateRandomApps.GetRandomGame(_random.Next(2000, 2025));

            if (!apps.Contains(game) && game != null)
            {
                apps.Add(game);
            }
        }
        
        countGames = apps.Count;

        if (countGames == 0)
        {
            Console.WriteLine("No games found");
            return;
        }
        
        _mainWindowFactory.ChangeGrid(countGames, GamesGrid);
        (List<Button> buttons, List<Image> images) = _mainWindowFactory.CreateButtonInGrid(countGames, GamesGrid);
        
        AddListenerOnButtonClick(buttons);
        InitDictionaryWithComponents(buttons, images);
        
        for (int i = 0; i < apps.Count; i++)
        {
            if (apps[i] == null)
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
    
    private async void TakeGame(object? sender, RoutedEventArgs e)
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
    private void ClickBackButton(object? sender, RoutedEventArgs e)
    {
        
    }


    private async void ClickBackButtonRandom(object? sender, RoutedEventArgs e)
    {
        MainPanel.IsVisible = !MainPanel.IsVisible;
    }

    private void ClickBackButtonRandomFalse(object? sender, RoutedEventArgs e)
    {
        MainPanel.IsVisible = false;
    }

    private void ClickBackButtonProfile(object? sender, RoutedEventArgs e)
    {
        ProfileMain.IsVisible = !MainPanel.IsVisible;
    }
    
    private void ClickBackButtonProfileFalse(object? sender, RoutedEventArgs e)
    {
        ProfileMain.IsVisible = false;
    }
}