using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.UserSystem;
using Steamworks;

namespace GameRandom.SteamSDK.LobbySystem;

public class LobbyService
{
    [Inject] private UserData? _userData;
    [Inject] private DatabaseService? _databaseService;
    [Inject] private EventBus? _eventBus;

    private bool _isCreating = false;

    public LobbyService()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        EnsureDependenciesInitialized();
    }
    public async Task StartApp()
    {
        var lobbyContexts = await CheckCurrentConnectionOnLobby();
        
        if (lobbyContexts == null || lobbyContexts.Count == 0)
        {
            Logger.Error("Not found any lobby context");
            return;
        }

        var userLobbyCtx = lobbyContexts.FirstOrDefault(e => e.MemberID == _userData.ClientId.m_SteamID);

        if (userLobbyCtx == null)
        {
            Logger.Error("Not found any lobby context");
            return;
        }
        
        _userData.SetLobbyId(userLobbyCtx.LobbyID, userLobbyCtx);
        
        SendLobbyEvent(lobbyContexts);
    }
    public async Task CreateLobby()
    {
        if (_isCreating)
        {
            //Show window with creating warning
            return;
        }

        if (_userData.LobbyId > 0)
        {
            //To:Do делать предупреждение если Lobby уже созданно
            
            await DisconnectFromLobby();
        }
        
        _isCreating = true;
        
        Random rnd = new Random();
        long lobbyId = rnd.NextInt64(0, long.MaxValue);

        var isAddNewLobby = await _databaseService.AddItemAsync(new Lobbies
        {
            LobbyID = lobbyId,
            MemberCount = 1
        });

        if (!isAddNewLobby)
        {
            Logger.Error("Failed to create lobbies");
            _isCreating = false;
            return;
        }
        
        var cSteamId = _userData.ClientId;

        string nickname = SteamFriends.GetPersonaName(); //Next time added to documentation error from steamFriends
                                                         //where developer try get personal name with methonds GetPersonalNickname throw new memory allocate Exception
        
        var lobbyContext = new LobbyContext
        {
            LobbyID = lobbyId,
            MemberID = cSteamId.m_SteamID,
            NickName = nickname
        };
        
        var isAddCurrentUserToLobby = await _databaseService.AddItemAsync(lobbyContext);

        if (!isAddCurrentUserToLobby)
        {
            Logger.Error($"Failed to add new member to lobby {lobbyId}");
            _isCreating = false;
            return;
        }
        
        _userData.SetLobbyId(lobbyId, lobbyContext);
        _eventBus.Publish(new LobbyUpdate(new List<LobbyContext>
        {
            lobbyContext
        }));

        _isCreating = false;
    }
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == 0) //To:Do show warning
            return;

        var lobbyList = await _databaseService.GetTableListAsync<Lobbies>();

        if (lobbyList == null) //Добавить ошибку: не найдено лобби в списке
            return;
        
        Lobbies? lobby = CheckLobby(lobbyList, lobbyId);

        if (lobby == null)
        {
            Logger.Error($"Failed to connect to {lobbyId}. Lobby not found");
            return;
        }
        
        var cSteamId = SteamManager.GetSteamManager().GetSteamId();

        var isAdded = await AddNewUser(lobbyId, cSteamId);

        if (isAdded)
        {
            lobby.MemberCount++;
            await _databaseService.UpdateAsync(lobby);
            Logger.Debug($"User {cSteamId.m_SteamID} joined the lobby {lobbyId}");
        }
    }
    public async Task DisconnectFromLobby()
    {
        if (_userData.LobbyId == 0 || _userData.CurrentLobbyContext == null)
            return;

        var currentLobbyData = await _databaseService.Where<Lobbies>(e => e.LobbyID == _userData.LobbyId);

        if (IsEmpty(currentLobbyData))
        {
            Logger.Error($"Failed to disconnect from {_userData.LobbyId}");
            return;
        }

        var isDeleted = await _databaseService.DeleteItemAsync<LobbyContext>(_userData.CurrentLobbyContext);

        if (isDeleted)
        {
            currentLobbyData.FirstOrDefault(e => e.LobbyID == _userData.LobbyId).MemberCount--;
        }
    }
    private async Task<bool> AddNewUser(long lobbyId, CSteamID cSteamId)
    {
        bool isAddNewUserToLobby = await _databaseService.AddItemAsync(new LobbyContext
        {
            LobbyID = lobbyId,
            MemberID = cSteamId.m_SteamID,
            NickName = SteamFriends.GetPlayerNickname(cSteamId)
        });

        return isAddNewUserToLobby;
    }
    private Lobbies? CheckLobby(List<Lobbies> lobbies, long lobbyId)
    {
        foreach (var lobby in lobbies)
        {
            if (lobby.LobbyID == lobbyId)
                return lobby;
        }

        return null;
    }
    public async Task<List<LobbyContext>?> GetLobbies(long lobbyId)
    {
        var allLobby = await _databaseService.GetTableListAsync<LobbyContext>();

        if (allLobby == null || allLobby.Count == 0)
        {
            Logger.Debug("Not lobbies found");
            return null;
        }
        
        return allLobby.Where(e => e.LobbyID == lobbyId).ToList();
    }
    private void SendLobbyEvent(List<LobbyContext>? lobbies)
    {
        if (lobbies == null || lobbies.Count == 0)
        {
            Logger.Error("SendLobbyEvent: Not lobbies found");
            return;
        }
        
        _eventBus.Publish(new LobbyUpdate(lobbies));
    }
    private async Task<List<LobbyContext>?> CheckCurrentConnectionOnLobby()
    {
        ulong clientId = SteamManager.GetSteamManager().GetSteamId().m_SteamID;

        var lobbyContext = await _databaseService.Where<LobbyContext>(e => e.MemberID == clientId);
        
        return lobbyContext;
    }
    private void EnsureDependenciesInitialized()
    {
        _userData = _userData ?? throw new InvalidOperationException("UserData not initialized");
        _databaseService = _databaseService ?? throw new InvalidOperationException("DatabaseService not initialized");
        _eventBus = _eventBus ?? throw new InvalidOperationException("EventBus not initialized");
    }
    private bool IsEmpty<T>(List<T>? list)
    {
        return list == null || list.Count == 0;
    }
}