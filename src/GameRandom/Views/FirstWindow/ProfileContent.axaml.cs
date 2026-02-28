using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : UserControl, IDisposable
{
    private Action<string>? _changeContent;
    private StatisticControl _statisticContent;
    private CurrentGame _currentGame;

    public ProfileContent()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            StatisticContent.Content = new StatisticControl();
            return;
        }
         
        
        DataContext = new ProfileViewModel();
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;

    public void Open()
    {
        InitProfileAvatar();
        
        _statisticContent = new StatisticControl();
        _statisticContent.Open();
        
        StatisticContent.Content = _statisticContent;
    }

    public void Close(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Main");
        Dispose();
    }

    private void InitProfileAvatar()
    {
        CSteamID steamId = SteamManager.GetSteamManager().GetSteamId();
        
        string accName = SteamFriends.GetPersonaName();
        
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);
        
        var bitmap = AvaloniaService.CreateSteamImage(imageId);
        
        ProfileImage.Source = bitmap;
        ProfileName.Text = accName;
    }

    public void Dispose()
    {
        _changeContent = null;
        ProfileImage.Source = null;
        DataContext = null;
    }

    private void OpenActivityGame(object? sender, RoutedEventArgs e)
    {
        _currentGame = new CurrentGame();
        _currentGame.Open();
    }
}