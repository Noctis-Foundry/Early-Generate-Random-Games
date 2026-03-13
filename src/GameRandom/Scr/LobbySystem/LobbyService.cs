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
        var user = GetCurrentUserAsync();

        var lobby = await FindLobbyAsync(user.LobbyId);
        SendLobbyEvent(lobby);
    }

    /// <summary>
    /// Create a new lobby
    /// </summary>
    public async Task CreateLobby()
    {
        if (_isCreating)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Lobby is creating", ErrorType = ErrorEnum.Message});
            return;
        }

        var user = GetCurrentUserAsync();

        // Disconnect from current lobby if user is already in one
        await DisconnectIfInLobby(user);

        _isCreating = true;

        try
        {
            long lobbyId = GenerateLobbyId();
            var lobbyData = CreateLobbyData(user, lobbyId);
            
            var admin = new Admins
            {
                SteamId = user.SteamId,
                LobbyId = lobbyId,
                IsTopAdmin = true
            };
            
            // Update lobby ID for the user
            if (!await User.GetInstance().UpdateLobbyId(lobbyId))
            {
                _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Failed update user lobby id. Stoping creating class...", ErrorType = ErrorEnum.Error});
                return;
            }

            var lobby = new Lobbies
            {
                LobbyId = lobbyId,
                LobbyData = new List<LobbyData> { lobbyData },
                AdminsList = new List<Admins> {admin},
                MembersCount = 1
            };

            // Save lobby to database
            if (!await _databaseService.AddItemAsync(lobby))
            {
                _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Failed to create lobbies", ErrorType = ErrorEnum.Error});
                await User.GetInstance().UpdateLobbyId(EmptyLobbyId);
                return;
            }
            
            User.GetInstance().SetAdminRules(lobby.AdminsList.FirstOrDefault(e => e.SteamId == user.SteamId) is not null);
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
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Player don't have a lobby", ErrorType = ErrorEnum.Warning});
            return;
        }

        var user = GetCurrentUserAsync();

        // Disconnect from current lobby before connecting to a new one
        await DisconnectIfInLobby(user);

        var lobby = await FindLobbyAsync(lobbyId);
        if (lobby is null)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = $"Failed to connect to {lobbyId}. Lobby not found", ErrorType = ErrorEnum.Error});
            return;
        }

        // Add user to lobby
        if (await User.GetInstance().UpdateLobbyId(lobbyId))
        {
            lobby.LobbyData.Add(new LobbyData
            {
                UserId = user.SteamId,
                LobbyId = lobbyId
            });
            lobby.MembersCount = lobby.LobbyData.Count;
            if (!await _databaseService.UpdateAsync(lobby))
            {
                await User.GetInstance().UpdateLobbyId(EmptyLobbyId);
                _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Failed to update lobby data", ErrorType = ErrorEnum.Error});
                return;
            }
            
            Logger.Debug($"User {user.SteamId} joined the lobby {lobbyId}");
        }
    }

    /// <summary>
    /// Disconnect from current lobby
    /// </summary>
    public async Task DisconnectFromLobby()
    {
        var user = GetCurrentUserAsync();
        if (user.LobbyId == EmptyLobbyId) return;

        var currentLobby = await FindLobbyAsync(user.LobbyId);
        if (currentLobby is null)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = $"Failed to disconnect from {user.LobbyId}. Lobby is empty", ErrorType = ErrorEnum.Error});
            return;
        }

        // Update user status
        if (!await User.GetInstance().UpdateLobbyId(DisconnectedLobbyId)) return;
        
        // Remove user from lobby members list
        currentLobby.LobbyData.RemoveAll(e => e.UserId == user.SteamId);
        currentLobby.MembersCount = currentLobby.LobbyData.Count;

        // Delete empty lobby or update data
        if (currentLobby.MembersCount <= 0)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "No member in lobby. Deleting lobby from database", ErrorType = ErrorEnum.Message});
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
    private Users GetCurrentUserAsync()
    {
        var user = User.GetInstance().GetUserInfo();
        return user;
    }

    /// <summary>
    /// Find lobby by ID
    /// </summary>
    private async Task<Lobbies?> FindLobbyAsync(long lobbyId)
    {
        var lobby = await _databaseService.GetLobbyById(lobbyId);

        var isAdmin = lobby?.AdminsList.FirstOrDefault(e => e.SteamId == User.GetInstance().GetUserInfo().SteamId) is not null;
        User.GetInstance().SetAdminRules(isAdmin);
        
        if (lobby is null)
        {
            _errorService.ShowWindow(new ErrorStruct{ErrorMessage = "Cannot find lobbies data to database", ErrorType = ErrorEnum.Error});
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
        if (userInfo.LobbyId > EmptyLobbyId)
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
            UserId = userInfo.SteamId
        };
    }
}