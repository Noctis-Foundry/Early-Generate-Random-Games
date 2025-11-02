using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.DataBaseContexts;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : UserControl
{
    private Action<string>? _changeContent;
    private readonly LobbyService _lobbyService = new LobbyService();
    
    public ProfileContent()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        InitializePlayerProfile();
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;
    
    private void InitializePlayerProfile()
    {
        CSteamID steamId = SteamManager.GetSteamManager().GetSteamId();

        string accName = SteamFriends.GetPersonaName();
        
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);

        var bitmap = AvaloniaService.CreateSteamImage(imageId);
        
        AvatarImage.Source = bitmap;
        AccName.Content = accName;
    }

    private void ExitFromProfile(object? sender, RoutedEventArgs e)
    {
        _changeContent?.Invoke("Main");
    }
    
}