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

/// <summary>
/// Lobby management service for multiplayer game sessions
/// </summary>
public class LobbyService
{
    private bool _isCreating;

    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly ErrorService _errorService = null!;

    private const long EmptyLobbyId = 0;
    private const long DisconnectedLobbyId = -1;

    /// <summary>
    /// Application initialization: loading current user's lobby
    /// </summary>
    public async Task StartApp()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return;

        var lobby = await FindLobbyAsync(user.LobbyID);
        SendLobbyEvent(lobby);
    }

    /// <summary>
    /// Create a new lobby
    /// </summary>
    public async Task CreateLobby()
    {
        if (_isCreating)
        {
            _errorService.ShowErrorWindow("Lobby is creating", ErrorEnum.Message);
            return;
        }

        var user = await GetCurrentUserAsync();
        if (user is null) return;

        // Disconnect from current lobby if user is already in one
        await DisconnectIfInLobby(user);

        _isCreating = true;

        try
        {
            long lobbyId = GenerateLobbyId();
            var lobbyData = CreateLobbyData(user, lobbyId);
            
            // Update lobby ID for the user
            if (!await User.GetInstance().UpdateLobbyId(lobbyId))
            {
                _errorService.ShowErrorWindow("Failed update user lobby id. Stoping creating class...",
                    ErrorEnum.Error);
                return;
            }

            var lobby = new Lobbies
            {
                LobbyId = lobbyId,
                LobbyData = new List<LobbyData> { lobbyData },
                MembersCount = 1
            };

            // Save lobby to database
            if (!await _databaseService.AddItemAsync(lobby))
            {
                _errorService.ShowErrorWindow("Failed to create lobbies", ErrorEnum.Error);
                await User.GetInstance().UpdateLobbyId(EmptyLobbyId);
                return;
            }
            
            SendLobbyEvent(lobby);
        }
        finally
        {
            _isCreating = false;
        }
    }

    /// <summary>
    /// Connect to an existing lobby
    /// </summary>
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == EmptyLobbyId)
        {
            _errorService.ShowErrorWindow("Player don't have a lobby", ErrorEnum.Warning);
            return;
        }

        var user = await GetCurrentUserAsync();
        if (user is null) return;

        // Disconnect from current lobby before connecting to a new one
        await DisconnectIfInLobby(user);

        var lobby = await FindLobbyAsync(lobbyId);
        if (lobby is null)
        {
            _errorService.ShowErrorWindow($"Failed to connect to {lobbyId}. Lobby not found", ErrorEnum.Error);
            return;
        }

        // Add user to lobby
        if (await User.GetInstance().UpdateLobbyId(lobbyId))
        {
            lobby.LobbyData.Add(new LobbyData
            {
                UserId = user.SteamID,
                LobbyId = lobbyId
            });
            lobby.MembersCount = lobby.LobbyData.Count;
            if (!await _databaseService.UpdateAsync(lobby))
            {
                await User.GetInstance().UpdateLobbyId(EmptyLobbyId);
                _errorService.ShowErrorWindow("Failed to update lobby data", ErrorEnum.Error);
                return;
            }
            
            Logger.Debug($"User {user.SteamID} joined the lobby {lobbyId}");
        }
    }

    /// <summary>
    /// Disconnect from current lobby
    /// </summary>
    public async Task DisconnectFromLobby()
    {
        var user = await GetCurrentUserAsync();
        if (user is null || user.LobbyID == EmptyLobbyId) return;

        var currentLobby = await FindLobbyAsync(user.LobbyID);
        if (currentLobby is null)
        {
            _errorService.ShowErrorWindow($"Failed to disconnect from {user.LobbyID}. Lobby is empty", ErrorEnum.Error);
            return;
        }

        // Update user status
        if (!await User.GetInstance().UpdateLobbyId(DisconnectedLobbyId)) return;
        
        // Remove user from lobby members list
        currentLobby.LobbyData.RemoveAll(e => e.UserId == user.SteamID);
        currentLobby.MembersCount = currentLobby.LobbyData.Count;

        // Delete empty lobby or update data
        if (currentLobby.MembersCount <= 0)
        {
            _errorService.ShowErrorWindow("No member in lobby. Deleting lobby from database", ErrorEnum.Message);
            await _databaseService.DeleteItemAsync(currentLobby);
        }
        else
        {
            await _databaseService.UpdateAsync(currentLobby);
            SendLobbyEvent(currentLobby);
        }
    }

    /// <summary>
    /// Get current user with null check
    /// </summary>
    private async Task<Users?> GetCurrentUserAsync()
    {
        var user = await User.GetInstance().GetUserInfo();

        if (user is null)
        {
            _errorService.ShowErrorWindow("Not find user data in system", ErrorEnum.Error);
        }

        return user;
    }

    /// <summary>
    /// Find lobby by ID
    /// </summary>
    private async Task<Lobbies?> FindLobbyAsync(long lobbyId)
    {
        var lobby = await _databaseService.GetLobbyById(lobbyId);

        if (lobby is null)
        {
            _errorService.ShowErrorWindow("Cannot find lobbies data to database", ErrorEnum.Error);
            return null;
        }

        return lobby;
    }

    /// <summary>
    /// Send lobby update event
    /// </summary>
    private void SendLobbyEvent(Lobbies? lobbies)
    {
        if (lobbies is null) return;

        _eventBus.Publish(new LobbyUpdate(lobbies.LobbyData));
    }

    /// <summary>
    /// Disconnect from lobby if user is currently in one
    /// </summary>
    private async Task DisconnectIfInLobby(Users userInfo)
    {
        if (userInfo.LobbyID > EmptyLobbyId)
        {
            //TODO Give choose for player
            await DisconnectFromLobby();
        }
    }

    /// <summary>
    /// Generate unique ID for a new lobby
    /// </summary>
    private static long GenerateLobbyId()
    {
        return Random.Shared.NextInt64(1, long.MaxValue);
    }

    /// <summary>
    /// Create lobby data for a user
    /// </summary>
    private static LobbyData CreateLobbyData(Users userInfo, long lobbyId)
    {
        return new LobbyData
        {
            LobbyId = lobbyId,
            UserId = userInfo.SteamID
        };
    }
}