using System;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.LobbySystem;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Events;

namespace GameRandom.ViewModels;

public class CreateLobbyViewModel : ViewModelBase
{
    private const string DefaultIdMessage = "No find lobby id";
    [Inject] private CreateLobbyService? _system;

    private string _currentLobbyId = DefaultIdMessage;
    
    public string CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public CreateLobbyViewModel()
    {
        if (Di.Container.TryGetInstance<EventBus>() is EventBus eventBus)
            eventBus.Subscribe<LobbyIdUpdate>(e => GetCurrentId());
        
        GetCurrentId();
    }

    private void GetCurrentId()
    {
        if (_system == null || _system.CurrentLobbyId == 0)
        {
            CurrentLobbyID = DefaultIdMessage;
            return;
        }
            
        CurrentLobbyID = _system.CurrentLobbyId.ToString();
    }
}