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
        await Testing();
        
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
        
        Logger.Debug($"User enter to lobby with {userLobbyCtx.LobbyID}");
        
        _userData.SetLobbyId(userLobbyCtx.LobbyID, userLobbyCtx);
        
        SendLobbyEvent(await _databaseService.Where<LobbyUserContext>(e => e.LobbyID == userLobbyCtx.LobbyID));
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

        (bool isAdded, LobbyUserContext lobbyContext) = await AddNewUser(lobbyId, _userData.ClientId);

        if (!isAdded)
        {
            Logger.Error($"Failed to add new member to lobby {lobbyId}");
            _isCreating = false;
            return;
        }
        
        _userData.SetLobbyId(lobbyId, lobbyContext);
        _eventBus.Publish(new LobbyUpdate(new List<LobbyUserContext>
        {
            lobbyContext
        }));

        _isCreating = false;
    }
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == 0) //To:Do show warning
            return;

        if (_userData.LobbyId > 0)
        {
            await DisconnectFromLobby();
        }
        
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

        (bool isAdded, LobbyUserContext lobbyContext) = await AddNewUser(lobbyId, cSteamId);

        if (isAdded)
        {
            lobby.MemberCount++;
            await _databaseService.UpdateAsync(lobby);
            Logger.Debug($"User {cSteamId.m_SteamID} joined the lobby {lobbyId}");
            
            _userData.SetLobbyId(lobbyId, lobbyContext);
        }
        
        SendLobbyEvent(await _databaseService.GetTableListAsync<LobbyUserContext>());
    }
    public async Task DisconnectFromLobby()
    {
        await Testing();
        
        if (_userData.LobbyId == 0 || _userData.CurrentLobbyContext == null)
            return;
        
        var currentLobbyData = await _databaseService.Where<Lobbies>(e => e.LobbyID == _userData.LobbyId);

        if (IsEmpty(currentLobbyData))
        {
            Logger.Error($"Failed to disconnect from {_userData.LobbyId}");
            return;
        }

        var isDeleted = await _databaseService.DeleteItemAsync<LobbyUserContext>(_userData.CurrentLobbyContext);

        if (isDeleted)
        {
            var ctx = currentLobbyData.FirstOrDefault(e => e.LobbyID == _userData.LobbyId);
            ctx.MemberCount--;

            if (ctx.MemberCount <= 0)
            {
                Logger.Debug("No member in lobby. Deleted");
                await _databaseService.DeleteItemAsync(ctx);
                return;
            }
            
            await _databaseService.UpdateAsync(ctx);
        }
    }
    private async Task Testing()
    {
        var lobbies = await _databaseService.GetTableListAsync<Lobbies>();

        if (lobbies == null || lobbies.Count == 0)
        {
            throw new Exception("Lobbys is empty");
        }

        foreach (var item in lobbies)
        {
            Logger.Debug($"Item lobby {item.LobbyID} and member count: {item.MemberCount}");
        }
    }
    private async Task<(bool, LobbyUserContext)> AddNewUser(long lobbyId, CSteamID cSteamId)
    {
        var lobbyContext = new LobbyUserContext
        {
            LobbyID = lobbyId,
            MemberID = cSteamId.m_SteamID,
            NickName = SteamFriends.GetPersonaName(),
            PlayerIcon = SteamFriends.GetLargeFriendAvatar(cSteamId)
        };

        bool isAddNewUserToLobby = await _databaseService.AddItemAsync(lobbyContext);

        return (isAddNewUserToLobby, lobbyContext);
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
    public async Task<List<LobbyUserContext>?> GetLobbies(long lobbyId)
    {
        var allLobby = await _databaseService.GetTableListAsync<LobbyUserContext>();

        if (allLobby == null || allLobby.Count == 0)
        {
            Logger.Debug("Not lobbies found");
            return null;
        }
        
        return allLobby.Where(e => e.LobbyID == lobbyId).ToList();
    }
    private void SendLobbyEvent(List<LobbyUserContext>? lobbies)
    {
        if (lobbies == null || lobbies.Count == 0)
        {
            Logger.Error("SendLobbyEvent: Not lobbies found");
            return;
        }
        
        _eventBus.Publish(new LobbyUpdate(lobbies));
    }
    private async Task<List<LobbyUserContext>?> CheckCurrentConnectionOnLobby()
    {
        ulong clientId = _userData.ClientId.m_SteamID;

        var lobbyContext = await _databaseService.Where<LobbyUserContext>(e => e.MemberID == clientId);
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