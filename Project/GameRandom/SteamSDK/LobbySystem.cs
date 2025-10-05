using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK;

public class LobbySystem
{
    private const int MemberToGroup = 6;
    
    private Callback<LobbyCreated_t>? _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameLobbyJoinRequested_t>? _gameLobbyJoinRequested;
    
    private TaskCompletionSource<List<LobbyContext>>? _lobbyCreatedTcs;

    public LobbySystem()
    {
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoin);
    }
    
    public async Task<List<LobbyContext>> CreateLobby(List<LobbyContext>? lobbiesData = null)
    {
        _lobbyCreatedTcs = new TaskCompletionSource<List<LobbyContext>>();
        
        _lobbyCreated = new Callback<LobbyCreated_t>(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MemberToGroup);
        
        return await _lobbyCreatedTcs.Task;

        void OnLobbyCreated(LobbyCreated_t pCallback)
        {
            if (pCallback.m_eResult != EResult.k_EResultOK)
            {
                _lobbyCreatedTcs.SetException(new Exception("Lobby creation failed"));
                return;
            }
            
            CSteamID newLobbyId = new CSteamID(pCallback.m_ulSteamIDLobby);
            
            Dispatcher.UIThread.Post(async () =>
            {
                await using var db = new AppDbContext();

                if (lobbiesData != null)
                {
                    Console.WriteLine("Starting load data to new lobby...");
                    
                    foreach (var data in lobbiesData)
                    {
                        await db.LobbyContexts
                            .Where(x => x.MemberID == data.MemberID)
                            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LobbyID, newLobbyId.m_SteamID));
                    }
                }

                await db.SaveChangesAsync();
                _lobbyCreatedTcs.SetResult(await db.LobbyContexts.ToListAsync());
            });
        }
    }
    public bool MemberToJoin(ulong lobbyId)
    {
        CSteamID steamLobbyId = new CSteamID(lobbyId);
        
        int members = SteamMatchmaking.GetNumLobbyMembers(steamLobbyId);

        if (members <= 0)
        {
            return false;
        }
        
        SteamMatchmaking.JoinLobby(steamLobbyId);

        return true;
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Console.WriteLine("Failed to join lobby");
            return;
        }
        
        CSteamID steamLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID userId = SteamManager.Instance.GetSteamId();
        string userName = SteamFriends.GetPersonaName();
        
        Dispatcher.UIThread.Post(async () =>
        {
            await using var db = new AppDbContext();

            if (!db.LobbyContexts.Any(x => x.MemberID == userId.m_SteamID))
            {
                await db.LobbyContexts.AddAsync(new LobbyContext
                {
                    LobbyID = steamLobbyId.m_SteamID,
                    MemberID = userId.m_SteamID,
                    NickName = userName
                });
            }
            
            Console.WriteLine($"{userName} joined to lobby {steamLobbyId.m_SteamID}");
            
            await db.SaveChangesAsync();
        });
    }
    public void InviteToLobby()
    {
        CSteamID lobbyId;
        
        using (var db = new AppDbContext())
        {
            var hostData = db.LobbyContexts.First(x => x.MemberID == SteamManager.Instance.GetSteamId().m_SteamID);
            lobbyId = new CSteamID(hostData.LobbyID);
        }
        
        var members = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        Console.WriteLine($"Start invite to lobby {lobbyId.m_SteamID} members to lobby {members}");
        
        try
        {
            SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to invite to lobby: " + e.Message);
        }
    }
    
    void OnLobbyJoin(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
}