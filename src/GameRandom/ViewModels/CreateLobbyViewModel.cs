using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class CreateLobbyViewModel : ViewModelBase
{
    private const string DefaultIdMessage = "No find lobby id";

    private string _currentLobbyId;

    public string CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public CreateLobbyViewModel()
    {
        GetCurrentId();

        if (Di.Container.GetInstance<EventBus>() is EventBus eventBus)
        {
            eventBus.Subscribe<LobbyUpdate>(e => GetCurrentId());
        }
    }

    private void GetCurrentId()
    {
        var userInfo = User.GetInstance().GetUserInfo();
        CurrentLobbyID = userInfo.LobbyId > 0 ? userInfo.LobbyId.ToString() : DefaultIdMessage;
    }
}