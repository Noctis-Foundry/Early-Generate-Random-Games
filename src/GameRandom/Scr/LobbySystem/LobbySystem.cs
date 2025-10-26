using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.Events;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK;

public class LobbySystem : ILobbyService
{
    #region Params

    public ILobbyService Instance { get; set; }
    private IDatabaseService _databaseService;

    private const int MemberToGroup = 6;

    private Callback<LobbyCreated_t>? _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameLobbyJoinRequested_t>? _gameLobbyJoinRequested;

    private bool _isCreatingLobby = false;
    private EventBus? _eventBus;
    public ulong CurrentLobbyId { get; private set; } = 0;

    #endregion
    
    public LobbySystem()
    {
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoin);
        Instance = this;

        if (Di.Container.TryGetInstance<EventBus>() is EventBus eventBus)
        {
            _eventBus = eventBus;
        }
        if (Di.Container.TryGetInstance<DatabaseService>() is IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
    }

    public async Task CreateLobby(List<LobbyContext>? lobbiesData = null)
    {
        if (CurrentLobbyId != 0)
            return; // TO:DO Add accept from user, if true - clear latest lobby and creat new, else skip command
            
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

    #region Connect

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
            try
            {
                await using var db = new AppDbContext();
                
                bool exists = await db.LobbyContexts
                    .AnyAsync(x => x.MemberID == userId.m_SteamID);
            
                if (!exists)
                {
                    var newLobbyContext = new LobbyContext
                    {
                        LobbyID = steamLobbyId.m_SteamID,
                        MemberID = userId.m_SteamID,
                        NickName = userName
                    };

                    bool result = await _databaseService.AddItemAsync(newLobbyContext);
            
                    if (!result)
                    {
                        Logger.Error("Failed to add context to database");
                        return;
                    }

                    Logger.Debug($"{userName} joined to lobby {steamLobbyId.m_SteamID}");
            
                    // Сохраняем изменения через сервис (уже сделано в AddItemAsync)
                    // await db.SaveChangesAsync(); // ⚠️ УБЕРИ ЭТУ СТРОКУ - дублирование!
            
                    if (_eventBus != null)
                        _eventBus.Publish(new LobbyUpdate());
                }
                else
                {
                    Logger.Debug($"{userName} already exists in database");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UI thread operation: {ex.Message}");
            }
        });
    }

    void OnLobbyJoin(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    #endregion
    
    #region Leave

    void LeaveFromLobby()
    {
        // TO:DO logic for leave from party. Get list members, find current user with current lobby id and delete from table and after leave from steam lobby.

        if (CurrentLobbyId == 0)
        {
            Logger.Warning("No lobbies found");
            return;
        }
        
        CSteamID currentSteamLobby = new CSteamID(CurrentLobbyId);
        SteamMatchmaking.LeaveLobby(currentSteamLobby);
        OnLeaving(CurrentLobbyId, SteamManager.GetSteamManager().GetSteamId());
    }

    private void OnLeaving(ulong lobbyId, CSteamID memberId)
    {
        if (lobbyId == 0 || memberId == CSteamID.Nil)
            return;
        
        List<LobbyContext>? lobbiesData = _databaseService.GetTableListAsync<LobbyContext>().Result;

        if (lobbiesData == null || lobbiesData.Count == 0)
            return;
        
        var memberData = lobbiesData.FirstOrDefault(e => e.LobbyID == lobbyId && e.MemberID == memberId.m_SteamID);

        if (memberData == null)
        {
            Logger.Error($"Not find lobby with member {memberId.m_SteamID}");
            return;
        }

        _databaseService.DeleteItemAsync(memberData);
    }

    #endregion

    #region DeleteLobby

    public void DeleteLobby()
    {
        //TO:DO If user have leader to lobby and he want delete lobby, we clear database and steam lobby from other player and after delete lobby
    }
    
    private void OnDeleteLobby(){} //Action after deleted lobby

    #endregion
    
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