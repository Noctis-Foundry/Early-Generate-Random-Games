using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK;

public class LobbySystem : ILobbyService
{
    public ILobbyService Instance { get; set; }
    
    private const int MemberToGroup = 6;
    
    private Callback<LobbyCreated_t>? _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameLobbyJoinRequested_t>? _gameLobbyJoinRequested;
    
    private TaskCompletionSource<List<LobbyContext>>? _lobbyCreatedTcs;

    public LobbySystem()
    {
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoin);
        Instance = this;
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

    public void ConnectToLobby(uint lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ||
            SteamManager.Instance == null)
        {
            var error = Di.Container.GetInstance<IError>() as ErrorService;
            error?.ShowErrorWindow("Not found room");
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
    public async Task<List<Users>> GetPartyMembers(uint lobbyId)
    {
        var users = new List<Users>();
        
        await using (var db = new AppDbContext())
        {
            var list = db.LobbyContexts.Where(x => x.LobbyID == lobbyId).ToList();

            foreach (var item in list)
            {
                Users user = db.Users.First(a => a.ClientID == item.MemberID);
                users.Add(user);
            }
        }

        return users;
    }
}

public interface ILobbyService
{
    Task<List<Users>> GetPartyMembers(uint lobbyId);
}