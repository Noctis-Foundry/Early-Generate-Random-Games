using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.DataBaseContexts;
using GameRandom.Service;
using GameRandom.SteamSDK;
using Steamworks;

namespace GameRandom.Views;

public partial class ProfileContent : UserControl
{
    private Action<string> _changeContent;
    private LobbySystem _lobbySystem = new LobbySystem();
    
    public ProfileContent()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        InitializePlayerProfile();
    }
    
    public void AddListener(Action<string> _onChangeContent) => _changeContent = _onChangeContent;

    public void CreateLobby(object? sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                Console.WriteLine("Stating create lobby...");
              
                var list = await _lobbySystem.CreateLobby();

                foreach (var item in list)
                {
                    Console.WriteLine($"Lobby = {item.LobbyID} and NickName = {item.NickName}");
                    await using var db = new AppDbContext();
                    db.Remove(item);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Lobby create failed: " + exception.Message);
                throw;
            }
        });
    }
    
    private void InitializePlayerProfile()
    {
        var clientID = SteamManager.Instance.GetSteamId();

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