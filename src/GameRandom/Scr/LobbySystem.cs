using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.SteamSDK.Events;
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

    private bool _isCreatingLobby = false;
    private EventBus? _eventBus;
    public ulong CurrentLobbyId { get; private set; } = 0;

    public LobbySystem()
    {
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoin);
        Instance = this;

        if (Di.Container.TryGetInstance<EventBus>() is EventBus eventBus)
        {
            _eventBus = eventBus;
        }
    }

    public async Task CreateLobby(List<LobbyContext>? lobbiesData = null)
    {
        if (_isCreatingLobby)
            return;

        _isCreatingLobby = true;

        _lobbyCreated?.Unregister();
        _lobbyCreated = new Callback<LobbyCreated_t>(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MemberToGroup);

        return;

        void OnLobbyCreated(LobbyCreated_t pCallback)
        {
            if (pCallback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }

            CSteamID newLobbyId = new CSteamID(pCallback.m_ulSteamIDLobby);
            CurrentLobbyId = newLobbyId.m_SteamID;

            if (_eventBus != null)
                _eventBus.Publish(new LobbyIdUpdate());

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

                _isCreatingLobby = false;
            });
        }
    }

    public void ConnectToLobby(uint lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (CurrentLobbyId == callback.m_ulSteamIDLobby)
            return;
        
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            var error = Di.Container.GetInstance<IError>() as ErrorService;
            error?.ShowErrorWindow("Not found room");
            return;
        }

        CSteamID steamLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID userId = SteamManager.GetSteamManager().GetSteamId();
        string userName = SteamFriends.GetPersonaName();

        Dispatcher.UIThread.Post(async () =>
        {
            await using var db = new AppDbContext();
            
            if (!await db.LobbyContexts.AnyAsync(x => x.MemberID == userId.m_SteamID))
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
            
            if (_eventBus != null)
                _eventBus.Publish(new LobbyUpdate());
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