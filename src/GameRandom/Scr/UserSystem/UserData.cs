using System;
using System.Collections.Generic;
using Steamworks;

namespace GameRandom.SteamSDK.UserSystem;

public class UserData 
{
    private ulong _lobbyId;
    public CSteamID ClientId { get; private set; }
    public ulong LobbyId
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

    public void SetLobbyId(ulong lobbyId)
    {
        LobbyId = lobbyId;
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