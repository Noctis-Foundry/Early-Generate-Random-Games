using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.UserSystem;
using Steamworks;

namespace GameRandom.SteamSDK.LobbySystem;

public class LobbyService
{
    [Inject] private UserData? _userData;
    [Inject] private DatabaseService? _databaseService;

    private const ulong Max = long.MaxValue;

    private bool _isCreating;

    public LobbyService()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (_userData == null || _databaseService == null)
            throw new NullReferenceException();
    }

    public async Task StartApp()
    {
        if (_databaseService == null)
            throw new NullReferenceException();

        var lobbyContexts = await _databaseService.GetTableListAsync<LobbyContext>();

        if (lobbyContexts == null || lobbyContexts.Count == 0)
        {
            Logger.Error("Not found any lobby context");
            return;
        }
        
        foreach (var lobbyContext in lobbyContexts)
        {
            if (lobbyContext.MemberID == SteamManager.GetSteamManager().GetSteamId().m_SteamID)
            {
                //Вызов ивента с обновлением UI составляющей программы, начиная от обновление объектов групп, заканчивая обновлением таблицы, внесением всех игроков.
            }
        }
    }
    public async Task CreateLobby()
    {
        //To:Do делать предупреждение если Lobby уже созданно
        
        if (_databaseService == null)
            throw new NullReferenceException();
        
        if (_isCreating)
        {
            //Show window with creating warning
            return;
        }

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
            return;
        }
        
        var cSteamId = SteamManager.GetSteamManager().GetSteamId();
        
        var isAddCurrentUserToLobby = await _databaseService.AddItemAsync(new LobbyContext
        {
            LobbyID = lobbyId,
            MemberID = cSteamId.m_SteamID,
            NickName = SteamFriends.GetPlayerNickname(cSteamId)
        });

        if (!isAddCurrentUserToLobby)
        {
            Logger.Error($"Failed to add new member to lobby {lobbyId}");
        }
    }
    public async Task ConnectToLobby(long lobbyId)
    {
        if (lobbyId == 0 || _databaseService == null) //To:Do show warning
            return;

        var lobbyList = await _databaseService.GetTableListAsync<Lobbies>();

        if (lobbyList == null) //Ошибка: не найдено лобби в списке
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
    private async Task<bool> AddNewUser(long lobbyId, CSteamID cSteamId)
    {
        if (_databaseService == null)
        {
            throw new NullReferenceException();
        }
        
        bool isAddNewUserToLobby = await _databaseService.AddItemAsync(new LobbyContext
        {
            LobbyID = lobbyId,
            MemberID = cSteamId.m_SteamID,
            NickName = SteamFriends.GetPlayerNickname(cSteamId)
        });

        return true;
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
}