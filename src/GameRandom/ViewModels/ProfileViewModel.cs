using Avalonia.Media.Imaging;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.ViewModels.BaseClasses;
using Steamworks;

namespace GameRandom.ViewModels;

using ViewModelBase = ViewModelBase;

public class ProfileViewModel : ViewModelBase
{
    private Bitmap? _profileImage;
    public Bitmap? ProfileImage
    {
        get => _profileImage;
        set => SetProperty(ref _profileImage, value);
    }

    private string? _playerNickname;
    public string? PlayerNickname
    {
        get => _playerNickname;
        set => SetProperty(ref _playerNickname, value);
    }
    
    public ProfileViewModel()
    {
        InitializeProfileContent();
    }

    private void InitializeProfileContent()
    {
        CSteamID steamId = SteamManager.GetSteamManager().GetSteamId();
        
        string accName = SteamFriends.GetPersonaName();
        
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);
        
        var bitmap = AvaloniaService.Instance.CreateSteamImage(imageId);
        
        if (string.IsNullOrEmpty(accName))
            accName = "Unknown";
        
        if (bitmap is null)
            bitmap = AvaloniaService.Instance.DefaultUserImage();
        
        ProfileImage = bitmap;
        PlayerNickname = accName;
    }

    public override void Dispose()
    {
        ProfileImage?.Dispose();
        PlayerNickname = null;
        
        base.Dispose();
    }
}