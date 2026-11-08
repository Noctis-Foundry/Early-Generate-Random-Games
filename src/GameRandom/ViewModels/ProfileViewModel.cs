using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GameRandom.DISystem;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Scripts.SteamSDK.SteamWebAPI;
using GameRandom.ViewModels.BaseClasses;
using Steamworks;

namespace GameRandom.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    [Inject] private ISteamWebService _steamWebService;
    [Inject] private SteamService _steamService;
    
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
        InitializeSemaphoreSlim();
        
        Dispatcher.UIThread.InvokeAsync( async() =>
        {
            await InitializeProfileContent();
        });
    }

    private async Task InitializeProfileContent()
    {
        InitializeDiContainer();
        CSteamID steamId = SteamManager.GetSteamManager().GetSteamId();
        InitializeProfileFromSteamSdk(steamId);
        
        if (!await SemaphoreSlim.WaitAsync(0))
        {
            Logger.Debug("Failed to start updating data, thread is not empty");
            return;
        }
        
        StartTaskWaiter();

        try
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var profileContext = await _steamWebService.GetProfile(steamId.m_SteamID, tokenSource.Token);

            if (profileContext == null)
            {
                ErrorService.ShowWindow("Failed to fetch profile");
                return;
            }

            var bitmap = await _steamService.GetImage(profileContext.avatar184x184, tokenSource.Token);

            if (bitmap is null)
            {
                Logger.Error("Failed to fetch user image");
                return;
            }

            ProfileImage = bitmap;

            if (string.IsNullOrEmpty(PlayerNickname) && string.IsNullOrEmpty(profileContext.nickname))
                PlayerNickname = "unknown";
            else if (!string.IsNullOrEmpty(PlayerNickname) && !string.IsNullOrEmpty(profileContext.nickname))
                PlayerNickname = profileContext.nickname;
        }
        catch (Exception e)
        {
            Logger.Debug(e.Message);
            ErrorService.ShowWindow("Failed to fetch profile, getting data from local files");
        }
        finally
        {
            CloseTaskWaiterWithSemaphore();
        }
    }

    private void InitializeProfileFromSteamSdk(CSteamID steamId)
    {
        var image = SteamFriends.GetLargeFriendAvatar(steamId);

        if (image != 0)
        {
            var bitmap = AvaloniaService.Instance.CreateSteamImage(image);
            
            if (bitmap != null)
                ProfileImage = bitmap;

            var nickname = SteamFriends.GetPersonaName();
            
            if (!string.IsNullOrEmpty(nickname))
                PlayerNickname = nickname;
        }
    }
    
    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_steamService == null)
            throw new NullReferenceException(nameof(_steamService));
        if (_steamWebService == null)
            throw new NullReferenceException(nameof(_steamWebService));
    }

    public override void Dispose()
    {
        ProfileImage?.Dispose();
        PlayerNickname = null;
        
        base.Dispose();
    }
}