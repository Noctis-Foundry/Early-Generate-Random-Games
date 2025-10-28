using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Events;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.Scr.LobbySystem;

public class CreateLobbyService
{
    #region Singleton

    private static CreateLobbyService? _instance;
    private static readonly System.Threading.Lock Lock = new();

    public static CreateLobbyService Instance
    {
        get
        {
            lock (Lock)
            {
                return _instance ??= new CreateLobbyService();
            }
        }
    }

    #endregion

    private const int MemberToGroup = 6;

    private Callback<LobbyCreated_t>? _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;

    private bool _isCreatingLobby = false;

    //private bool _isPlayerJoined = false; TO:DO For check, user lobby connect
    public ulong CurrentLobbyId { get; private set; } = 0;

    public CreateLobbyService()
    {
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    [Inject] private EventBus? _eventBus;
    [Inject] private DatabaseService? _databaseService;
    [Inject] private ErrorService? _errorService;

    public void CreateLobby()
    {
        if (CurrentLobbyId != 0)
            return; // TO:DO Add accept from user, if true - clear latest lobby and creat new, else skip command

        if (_isCreatingLobby)
            return;

        _isCreatingLobby = true;

        _lobbyCreated?.Unregister();
        _lobbyCreated = new Callback<LobbyCreated_t>(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MemberToGroup);
    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        Logger.Info($"CreateLobby: _eventBus is null: {_eventBus == null}, this: {this.GetHashCode()}");

        if (!_isCreatingLobby)
            return;

        if (pCallback.m_eResult != EResult.k_EResultOK)
        {
            _isCreatingLobby = false;
            return;
        }

        CSteamID newLobbyId = new CSteamID(pCallback.m_ulSteamIDLobby);
        CurrentLobbyId = newLobbyId.m_SteamID;

        Logger.Error($"Lobby created: {CurrentLobbyId}");

        if (_eventBus != null)
            _eventBus.Publish(new LobbyIdUpdate());
        else
            Logger.Error("Not  found _eventBus");

        UpdateLobbyDataInDatabase(newLobbyId); //TO:DO Problems with creating lobby. App stoping and cant unlagging.
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (!_isCreatingLobby)
        {
            Logger.Debug($"Lobby is not creating");
            return;
        }

        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            _errorService?.ShowErrorWindow("Not found room");
            return;
        }

        CSteamID steamLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        ;
        CSteamID userId = SteamManager.GetSteamManager().GetSteamId();
        string userName = SteamFriends.GetPersonaName();

        UpdateLobbyDataAfterEnteredToLobby(steamLobbyId, userId, userName);
    }

    private void UpdateLobbyDataInDatabase(CSteamID steamLobbyId)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await using var db = new AppDbContext();
                
                List<LobbyContext>? lobbiesData = _databaseService?.GetTableListAsync<LobbyContext>().Result;

                if (lobbiesData != null)
                {
                    Console.WriteLine("Starting load data to new lobby...");

                    foreach (var data in lobbiesData)
                    {
                        await db.LobbyContexts
                            .Where(x => x.MemberID == data.MemberID)
                            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LobbyID, steamLobbyId.m_SteamID));
                    }

                    await db.SaveChangesAsync();
                }

                _isCreatingLobby = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
    }

    private void UpdateLobbyDataAfterEnteredToLobby(CSteamID steamLobbyId, CSteamID userId, string userName)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
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


                    if (_eventBus != null)
                        _eventBus.Publish(new LobbyUpdate());
                    else
                        Logger.Error("Event bus to Lobby entered = null");
                }
                else
                {
                    Logger.Debug($"{userName} already exists in database");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UI thread operation: {ex}");
            }
        });
    }
}