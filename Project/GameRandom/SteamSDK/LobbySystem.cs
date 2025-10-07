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
                    
                    await db.SaveChangesAsync();
                }
                
                _lobbyCreatedTcs.SetResult(await db.LobbyContexts.ToListAsync());
            });
        }
    }
    public string MemberToJoin(ulong lobbyId)
    {
        try
        {
            CSteamID steamLobbyId = new CSteamID(lobbyId);
            SteamMatchmaking.JoinLobby(steamLobbyId);
            
            return "Connected to lobby";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
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
    void OnLobbyJoin(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
}