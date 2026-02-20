using System;
using System.Linq;
using GameRandom.DataBaseContexts;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK.UserData;

public class User
{
    [Inject] private DatabaseService? _databaseService;

    private static Lazy<User> _userInstance = new (() => new User());
    public static User GetInstance() => _userInstance.Value;

    private bool isInitialized = false;
    
    private User()
    {
        
    }

    private Users? _userInfo;

    public async Task InitializeUser()
    {
        if (_userInfo is not null || isInitialized) return;
        
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null) throw new NullReferenceException();
        
        _userInfo = await _databaseService.GetUserByUlongId(SteamManager.GetSteamIdAsLong());

        if (_userInfo is not null)
        {
            Console.WriteLine($"User already exists in DB. Nickname: {_userInfo.Nickname}");
            return;
        }
        
        if (_userInfo is null)
        {
            var user = new Users()
            {
                SteamID = SteamManager.GetSteamIdAsLong(),
                Nickname = SteamFriends.GetPersonaName(),
                LobbyID = 0,
                AvatarURL = SteamFriends.GetLargeFriendAvatar(SteamManager.GetSteamManager().GetSteamId())
            };

            _userInfo = user;

            bool isAdding = await _databaseService.AddItemAsync(_userInfo);
            
            if (isAdding)
                Console.WriteLine("New user added to DB");
            else
            {
                throw new Exception("Failed to add new user to database");
            }
        }

        isInitialized = _userInfo is not null;
    }

    public async Task<bool> UpdateLobbyId(long lobbyId)
    {
        if (_userInfo is null) return false;
        
        _userInfo.LobbyID = lobbyId;

        if (Di.Container.GetInstance<DatabaseService>() is DatabaseService service)
        {
            bool isUpdating = await service.UpdateAsync(_userInfo);

            return isUpdating;
        }

        return false;
    }

    public async Task<Users?> GetUserInfo()
    {
        if (_userInfo is not null)
            return _userInfo;
        
        await using var dbContext = new AppDbContext();
        var user = dbContext.Users.FirstOrDefault(e => e.SteamID == SteamManager.GetSteamIdAsLong());

        return user;
    }
}