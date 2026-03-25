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
    [Inject] private DatabaseService? _databaseService;
    [Inject] private PostgresListener? _postgresListener;

    private static Lazy<User> _userInstance = new (() => new User());
    private bool _isInitialized = false;
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
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null)
            throw new NullReferenceException("Database service is null");
        
        if (_postgresListener is null)
            throw new NullReferenceException("Postgres listener is null");
        
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

        _postgresListener.Subscribe(TableEnum.AdminTable, e =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (e.TableCode != (int)TableEnum.AdminTable)
                    return;
                
                await UpdateAdminRules();
            });
        });

        await UpdateAdminRules();
        
        _isInitialized = true;
    }

    /// <summary>
    /// Update lobby ID for current user
    /// </summary>
    public async Task<bool> UpdateLobbyId(long lobbyId)
    {
        if (_databaseService is null) return false;
        
        _userInfo.LobbyId = lobbyId;
        
        bool isUpdating = await _databaseService.UpdateAsync(_userInfo);

        return isUpdating;
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

        bool isAdding = await _databaseService.AddItemAsync(_userInfo);
        
        if (!isAdding)
            throw new Exception("Failed to add new user to database");

        Console.WriteLine("New user added to DB");

        var isUserGame = await _databaseService.TryGetOrCreateUserGame(_userInfo);
        
        if (!isUserGame)
            throw new Exception("Failed to create user game");
            
    }
    
    /// <summary>
    /// Get current user information
    /// </summary>
    public Users GetUserInfo() => _userInfo;
    
    public ulong GetUserId() => _userInfo.SteamId;
    
    public bool IsAdmin() => _isAdmin;

    public bool IsTopLevelAdmin() => _isAdmin && _isTopLevelAdmin;
}