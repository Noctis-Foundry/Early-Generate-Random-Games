using System;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK.UserSystem;

namespace GameRandom.ViewModels;

public class CreateLobbyViewModel : ViewModelBase
{
    [Inject] private UserData? _userData;
    private const string DefaultIdMessage = "No find lobby id";

    private string _currentLobbyId;
    
    public string CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public CreateLobbyViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_userData == null)
            throw new Exception("UserData is null");
        
        _userData?.LobbyIdUpdated += GetCurrentId;
        GetCurrentId();
    }
    
    private void GetCurrentId()
    {
        if (_userData == null || _userData.LobbyId == 0)
        {
            CurrentLobbyID = DefaultIdMessage;
            return;
        }

        CurrentLobbyID = _userData.LobbyId.ToString();
    }
}