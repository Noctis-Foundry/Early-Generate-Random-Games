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
using GameRandom.SteamSDK.UserData;

namespace GameRandom.SteamSDK.LobbySystem;

public class LobbyService
{
    private bool _isCreating = false;
    
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly ErrorService _errorService = null!;
    private Users? _userData;
    
    public async Task StartApp()
    {
        
        _userData = await User.GetInstance().GetUserInfo();

        if (_userData is null)
        {
            _errorService.ShowErrorWindow("Not find user data in system", ErrorEnum.Error);
            return;
        }
        
        SendLobbyEvent(await _databaseService.GetLobbyById(_userData.LobbyID));
    }
    public async Task CreateLobby()
    {
        if (_isCreating)
        {
            _errorService.ShowErrorWindow("Lobby is creating", ErrorEnum.Message);
            return;
        }
        
        if (_userData.LobbyID > 0)
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
            MembersCount = 1
        });

        if (!isAddNewLobby)
        {
            _errorService.ShowErrorWindow("Failed to create lobbies", ErrorEnum.Error);
            _isCreating = false;
            return;
        }

        _userData.LobbyID = lobbyId;
        bool isAdded = await _databaseService.UpdateAsync(_userData);
        
        if (!isAdded)
        {
            _errorService.ShowErrorWindow("Failed to add new lobby context", ErrorEnum.Error);
            _isCreating = false;
            return;
        }
        
        _isCreating = false;
    }
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == 0)
        {
            _errorService.ShowErrorWindow("Player don't have a lobby", ErrorEnum.Warning);
        }

        if (_userData.LobbyID > 0)
        {
            //To:Do add accept window
            await DisconnectFromLobby();
        }
        
        var lobbyList = await _databaseService.GetTableListAsync<Lobbies>();

        if (lobbyList == null)
        {
            _errorService.ShowErrorWindow("Cannot find lobbies data to database", ErrorEnum.Error);
            return;
        }
        
        Lobbies? lobby = CheckLobby(lobbyList, lobbyId);

        if (lobby == null)
        {
            _errorService.ShowErrorWindow($"Failed to connect to {lobbyId}. Lobby not found", ErrorEnum.Error);
            return;
        }
        
        var cSteamId = SteamManager.GetSteamManager().GetSteamId();

        _userData.LobbyID = lobbyId;
        bool isAdded = await _databaseService.UpdateAsync(_userData);

        if (isAdded)
        {
            lobby.MembersCount++;
            await _databaseService.UpdateAsync(lobby);
            Logger.Debug($"User {cSteamId.m_SteamID} joined the lobby {lobbyId}");
        }
    }
    public async Task DisconnectFromLobby()
    {
        if (_userData.LobbyID == 0)
            return;
        
        var lobbiesData = await _databaseService.GetTableListAsync<Lobbies>();
        var currentLobby = lobbiesData?.FirstOrDefault(e => e.LobbyID == _userData.LobbyID);

        if (currentLobby is null)
        {
            _errorService.ShowErrorWindow($"Failed to disconnect from {_userData.LobbyID}. Lobby is empty", ErrorEnum.Error);
            return;
        }

        _userData.LobbyID = 0;
        bool isDeleted = await _databaseService.UpdateAsync(_userData);

        if (isDeleted)
        {
            var ctx = lobbiesData?.FirstOrDefault(e => e.LobbyID == _userData.LobbyID);

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
    
    private Lobbies? CheckLobby(List<Lobbies> lobbies, long lobbyId)
    {
        foreach (var lobby in lobbies)
        {
            if (lobby.LobbyID == lobbyId)
                return lobby;
        }
        
        return null;
    }
    
    private void SendLobbyEvent(Lobbies? lobbies)
    {
        if (lobbies is null) return;
        
        _eventBus.Publish(new LobbyUpdate(lobbies.LobbyData));
    }
    
    private bool IsEmpty<T>(List<T>? list)
    {
        return list == null || list.Count == 0;
    }
}