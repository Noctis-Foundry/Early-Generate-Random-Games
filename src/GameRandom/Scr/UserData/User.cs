using System;
using System.Linq;
using GameRandom.DataBaseContexts;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Steamworks;

namespace GameRandom.Src.UserData;

/// <summary>
/// Singleton class for managing current Steam user data
/// </summary>
public class User
{
    [Inject] private DatabaseService _databaseService = null!;
    [Inject] private DatabaseTransitionService _transitionService = null!;
    [Inject] private PostgresListener _postgresListener = null!;

    private static Lazy<User> _userInstance = new (() => new User());
    private Users _userInfo;
    
    private User() { }

    /// <summary>
    /// Get the single User instance
    /// </summary>
    public static User GetInstance() => _userInstance.Value;

    private bool _isAdmin = false;
    private bool _isTopLevelAdmin = false;

    /// <summary>
    /// Initialize user: load from DB or create new
    /// </summary>
    public async Task InitializeUser()
    {
        InitializeDependence();

        await GetUserDataOrCreate();

        InitializePostgresListener();

        await UpdateAdminRules();
    }

    private void InitializeDependence()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null)
            throw new NullReferenceException("Database service is null");
        
        if (_postgresListener is null)
            throw new NullReferenceException("Postgres listener is null");

        if (_transitionService is null)
            throw new NullReferenceException("Transition service is null");
    }
    
    /// <summary>
    /// Update lobby ID for current user
    /// </summary>
    public async Task<bool> UpdateLobbyId(long lobbyId)
    {
        _userInfo.LobbyId = lobbyId;
        
        return await _databaseService.UpdateAsync(_userInfo);
    }

    private async Task UpdateAdminRules()
    {
        if (Di.Container.GetInstance<EventBus>() is not EventBus bus)
            throw new NullReferenceException("Event bus is null");
        
        var admin = await _databaseService.GetFirstOrDefaultAsync<Admins>(e => e.SteamId == GetUserId());

        if (admin is null)
        {
            _isAdmin = false;
            _isTopLevelAdmin = false;
        }
        else
        {
            _isAdmin = true;
            _isTopLevelAdmin = admin.IsTopAdmin;
        }
        
        bus.Publish(new AdminRulesUpdating());
    }

    private async Task CreateUser()
    {
        var user = new Users()
        {
            SteamId = SteamManager.GetSteamIdAsLong(),
            Nickname = SteamFriends.GetPersonaName(),
            LobbyId = 0,
            AvatarURL = SteamFriends.GetLargeFriendAvatar(SteamManager.GetSteamManager().GetSteamId())
        };

        _userInfo = user;

        var isReady = await _transitionService.TransitionAddUser(user);

        if (!isReady)
            throw new NullReferenceException("failed to create user");
        else
            Logger.Debug("User is successes created");

    }

    private async Task GetUserDataOrCreate()
    {
        var userInfo = await _databaseService.GetUserByUlongId(SteamManager.GetSteamIdAsLong());

        // If user exists in DB
        if (userInfo is not null)
        {
            _userInfo = userInfo;
            
            var isAddUserGame = await _databaseService.TryGetOrCreateUserGame(_userInfo);
            
            if (!isAddUserGame)
                throw new Exception("Failed to add user game cell"); ;
        }
        else 
            await CreateUser();
    }
    
    private void InitializePostgresListener()
    {
        _postgresListener.Subscribe(TableEnum.AdminTable, e =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (e.TableCode != (int)TableEnum.AdminTable)
                    return;
                
                await UpdateAdminRules();
            });
        });
    }
    
    /// <summary>
    /// Get current user information
    /// </summary>
    public Users GetUserInfo() => _userInfo;
    
    public ulong GetUserId() => _userInfo.SteamId;
    
    public bool IsAdmin() => _isAdmin;

    public bool IsTopLevelAdmin() => _isAdmin && _isTopLevelAdmin;
}