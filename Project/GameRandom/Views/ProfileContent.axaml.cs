using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : UserControl
{
    private readonly Action<string> _changeContent;
    
    public ProfileContent(Action<string> changeContent)
    {
        InitializeComponent();
        InitializePlayerProfile();
        _changeContent = changeContent;
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

    private void ExitFromProfile(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Main");
    }

}