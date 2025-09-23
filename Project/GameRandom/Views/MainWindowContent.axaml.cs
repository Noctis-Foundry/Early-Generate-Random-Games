using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;

namespace GameRandom.Views;

public partial class MainWindowContent : UserControl
{
    private ContentControl _rollContent;
    private Action<bool> _changeRollContent;
    
    public MainWindowContent()
    {
        InitializeComponent();
        InitializePlayerProfile();
    }

    public void InitializeMainContent(Action<bool> changeRollContent)
    {
        _changeRollContent = changeRollContent;
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
    
    private void GoToRollContent(object? sender, RoutedEventArgs e)
    {
        _changeRollContent?.Invoke(true);
    }
   
    private void ClickBackButton(object? sender, RoutedEventArgs e)
    {
       
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