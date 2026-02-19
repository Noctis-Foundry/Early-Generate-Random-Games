using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserSystem;
using Steamworks;

namespace GameRandom.SteamSDK.LobbySystem;

public class LobbyService
{
    private bool _isCreating = false;
    
    [Inject] private readonly UserData _userData = null!;
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly ErrorService _errorService = null!;
    
    public async Task StartApp()
    {
        var lobbyContexts = await CheckCurrentConnectionOnLobby();
        var userLobbyCtx = lobbyContexts.FirstOrDefault(e => e.SteamID == _userData.ClientId.m_SteamID);

        if (userLobbyCtx == null)
        {
            _errorService.ShowErrorWindow("not find user in database", ErrorEnum.Error);
            return;
        }
        
        _userData.SetLobbyId(userLobbyCtx.LobbyID);
        _userData.SetLobbyContext(userLobbyCtx);
        
        SendLobbyEvent(await _databaseService.Where<User>(e => e.LobbyID == userLobbyCtx.LobbyID));
    }
    public async Task CreateLobby()
    {
        if (_isCreating)
        {
            _errorService.ShowErrorWindow("Lobby is creating", ErrorEnum.Message);
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

        var isAddNewLobby = await _databaseService.AddItemAsync(new Lobby
        {
            LobbyID = lobbyId,
            MembersCount = 1
        });

        if (!isAddNewLobby)
        {
            _errorService.ShowErrorWindow("Failed to create lobbies", ErrorEnum.Error);
            _isCreating = false;
            return;
        }
        
        _userData.SetLobbyId(lobbyId);
        (bool isAdded, User lobbyContext) = await AddNewUser(lobbyId, _userData.ClientId);
        
        if (!isAdded)
        {
            _errorService.ShowErrorWindow("Failed to add new lobby context", ErrorEnum.Error);
            _isCreating = false;
            return;
        }
        
        _userData.SetLobbyContext(lobbyContext);
        _isCreating = false;
    }
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == 0)
        {
            _errorService.ShowErrorWindow("Player don't have a lobby", ErrorEnum.Warning);
        }

        if (_userData.LobbyId > 0)
        {
            //To:Do add accept window
            await DisconnectFromLobby();
        }
        
        var lobbyList = await _databaseService.GetTableListAsync<Lobby>();

        if (lobbyList == null)
        {
            _errorService.ShowErrorWindow("Cannot find lobbies data to database", ErrorEnum.Error);
            return;
        }
        
        Lobby? lobby = CheckLobby(lobbyList, lobbyId);

        if (lobby == null)
        {
            _errorService.ShowErrorWindow($"Failed to connect to {lobbyId}. Lobby not found", ErrorEnum.Error);
            return;
        }
        
        var cSteamId = SteamManager.GetSteamManager().GetSteamId();

        _userData.SetLobbyId(lobbyId);
        (bool isAdded, User lobbyContext) = await AddNewUser(lobbyId, cSteamId);

        if (isAdded)
        {
            lobby.MembersCount++;
            await _databaseService.UpdateAsync(lobby);
            Logger.Debug($"User {cSteamId.m_SteamID} joined the lobby {lobbyId}");
            _userData.SetLobbyContext(lobbyContext);
        }
    }
    public async Task DisconnectFromLobby()
    {
        if (_userData.LobbyId == 0 || _userData.CurrentLobbyContext == null)
            return;
        
        var currentLobbyData = await _databaseService.Where<Lobby>(e => e.LobbyID == _userData.LobbyId);

        if (IsEmpty(currentLobbyData))
        {
            _errorService.ShowErrorWindow($"Failed to disconnect from {_userData.LobbyId}. Lobby is empty", ErrorEnum.Error);
            return;
        }

        var isDeleted = await _databaseService.DeleteItemAsync<User>(_userData.CurrentLobbyContext);

        if (isDeleted)
        {
            var ctx = currentLobbyData?.FirstOrDefault(e => e.LobbyID == _userData.LobbyId);

            if (ctx == null) return;

            ctx.MembersCount--;

            if (ctx.MembersCount <= 0)
            {
                _errorService.ShowErrorWindow("No member in lobby. Deleting lobby from database", ErrorEnum.Message);
                await _databaseService.DeleteItemAsync(ctx);
                return;
            }
            
            await _databaseService.UpdateAsync(ctx);
        }
    }
    private async Task<(bool, User)> AddNewUser(long lobbyId, CSteamID cSteamId)
    {
        var lobbyContext = new User
        {
            LobbyID = lobbyId, 
            SteamID = cSteamId.m_SteamID,
            Nickname = SteamFriends.GetPersonaName(),
            AvatarURL = SteamFriends.GetLargeFriendAvatar(cSteamId)
        };

        bool isAddNewUserToLobby = await _databaseService.AddItemAsync(lobbyContext);

        return (isAddNewUserToLobby, lobbyContext);
    }
    private Lobby? CheckLobby(List<Lobby> lobbies, long lobbyId)
    {
        foreach (var lobby in lobbies)
        {
            if (lobby.LobbyID == lobbyId)
                return lobby;
        }
        
        return null;
    }
    private void SendLobbyEvent(List<User>? lobbies)
    {
        if (IsEmpty(lobbies))
        {
            _errorService.ShowErrorWindow("SendLobbyEvent: Not lobbies found", ErrorEnum.Error);
            return;
        }
        
        _eventBus.Publish(new LobbyUpdate(lobbies));
    }
    private async Task<List<User>?> CheckCurrentConnectionOnLobby()
    {
        ulong clientId = _userData.ClientId.m_SteamID;

        var lobbyContext = await _databaseService.Where<User>(e => e.SteamID == clientId);
        return lobbyContext;
    }
    private bool IsEmpty<T>(List<T>? list)
    {
        return list == null || list.Count == 0;
    }
}