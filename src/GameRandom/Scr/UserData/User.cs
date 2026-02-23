using System;
using System.Linq;
using GameRandom.DataBaseContexts;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK.UserData;

/// <summary>
/// Singleton class for managing current Steam user data
/// </summary>
public class User
{
    [Inject] private DatabaseService? _databaseService;

    private static Lazy<User> _userInstance = new (() => new User());
    private bool _isInitialized = false;
    private Users _userInfo;
    
    private User() { }

    /// <summary>
    /// Get the single User instance
    /// </summary>
    public static User GetInstance() => _userInstance.Value;

    /// <summary>
    /// Initialize user: load from DB or create new
    /// </summary>
    public async Task InitializeUser()
    {
        Di.Container.ResolveField(out _databaseService);

        if (_databaseService is null)
        {
            throw new NullReferenceException("Database service is null");
        }
        
        var userInfo = await _databaseService.GetUserByUlongId(SteamManager.GetSteamIdAsLong());

        // If user exists in DB
        if (userInfo is not null)
        {
            _userInfo = userInfo;
            _isInitialized = true;
    
            var isAddUserGame = await _databaseService.AddUserGameAsync(_userInfo);
            
            if (!isAddUserGame)
                throw new Exception("Failed to add user game cell");
    
            Console.WriteLine($"User already exists in DB. Nickname: {_userInfo.Nickname}");
            return;
        }
        
        // Create new user
        var user = new Users()
        {
            SteamID = SteamManager.GetSteamIdAsLong(),
            Nickname = SteamFriends.GetPersonaName(),
            LobbyID = 0,
            AvatarURL = SteamFriends.GetLargeFriendAvatar(SteamManager.GetSteamManager().GetSteamId())
        };

        _userInfo = user;

        bool isAdding = await _databaseService.AddItemAsync(_userInfo);
        if (!isAdding)
            throw new Exception("Failed to add new user to database");

        Console.WriteLine("New user added to DB");

        var isCreatingUserGameCell = await _databaseService.AddUserGameAsync(_userInfo);
        _isInitialized = isCreatingUserGameCell;

        if (!_isInitialized)
            throw new Exception("Failed to initialize user data and user game cell");
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

    /// <summary>
    /// Get current user information
    /// </summary>
    public Users GetUserInfo() => _userInfo;
}