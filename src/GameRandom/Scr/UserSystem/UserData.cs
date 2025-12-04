using System;
using System.Collections.Generic;
using GameRandom.DataBaseContexts;
using Steamworks;

namespace GameRandom.SteamSDK.UserSystem;

public class UserData 
{
    private long _lobbyId;
    public CSteamID ClientId { get; private set; }
    public LobbyUserContext? CurrentLobbyContext { get; private set; }
    public long LobbyId
    {
        get => _lobbyId;
        private set
        {
            _lobbyId = value;
            LobbyIdUpdated?.Invoke();
        }
        
    }

    public Action LobbyIdUpdated;

    private List<IObserver<UserCtx>> _observers;
    
    public UserData(CSteamID clientId)
    { 
        ClientId = clientId;
    }

    public void SetLobbyId(long lobbyId)
    {
        LobbyId = lobbyId;
    }

    public void SetLobbyContext(LobbyUserContext lobbyContext)
    {
        CurrentLobbyContext = lobbyContext;
    }
    
    public void UnsetLobby()
    {
        LobbyId = 0;
        CurrentLobbyContext = null;
    }
}

public class UserCtx
{
    public CSteamID ClientId { get; private set; }
    public ulong LobbyId { get; private set; }

    public UserCtx(CSteamID clientId, ulong lobbyId)
    {
        ClientId = clientId;
        LobbyId = lobbyId;
    }
}