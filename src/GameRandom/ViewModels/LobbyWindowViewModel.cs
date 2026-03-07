using Avalonia.Controls;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class LobbyWindowViewModel : ViewModelBase
{
    private const long  DefaultIdMessage = 0;

    private long  _currentLobbyId;

    public long CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public LobbyWindowViewModel()
    {
        if (Design.IsDesignMode)
            return;
        
        GetCurrentId();

        if (Di.Container.GetInstance<EventBus>() is EventBus eventBus)
        {
            eventBus.Subscribe<LobbyUpdate>(e => GetCurrentId());
        }
    }

    private void GetCurrentId()
    {
        var userInfo = User.GetInstance().GetUserInfo();
        CurrentLobbyID = userInfo.LobbyId > 0 ? userInfo.LobbyId: DefaultIdMessage;
    }
}