using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using GameRandom.CoreApp;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;
using AppContext = GameRandom.CoreApp.AppContext;

namespace GameRandom.Views;

public partial class MainWindow : Window
{
    private readonly GenerateRandomApps _generateRandomApps = new GenerateRandomApps();
    private Dictionary<int, Image> _imageData = new();
    public MainWindow()
    {
        InitializeComponent();
        
        InitializePlayerProfile();
        
        _imageData.Add(1, AppImage1);
        _imageData.Add(2, AppImage2);
        _imageData.Add(3, AppImage3);
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
        List<AppFilterResult> apps = await _generateRandomApps.GenerateApps(new AppFilterSend("RPG", 3));

        if (apps.Count <= 0)
        {
            Console.WriteLine("No apps found");
            return;
        }

        int imageCount = 1;
        
        foreach (var app in apps)
        {
            Bitmap? bitmap = await SteamService.Instance.GetImage(app.AppData.ImageUrl);

            if (bitmap == null)
            {
                Console.WriteLine("No image found");
                continue;
            }

            if (_imageData.TryGetValue(imageCount, out Image? image))
            {
                image.Source = bitmap;
            }
            
            imageCount++;
        }
    }

    private void ClickBackButton(object? sender, RoutedEventArgs e)
    {
        
    }
}