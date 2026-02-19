using System.Linq;
using GameRandom.DataBaseContexts;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using Steamworks;

namespace GameRandom.SteamSDK.UserData;

public class User
{
    private static User _userInstance = new User();
    
    public static User GetInstance() => _userInstance;

    private Users? _userInfo;

    public async Task InitializeUser()
    {
        if (_userInfo is not null) return;
        
        await using var dbContext = new AppDbContext();
        var list = dbContext.Users.ToList();
        _userInfo = list.FirstOrDefault(e => e.SteamID == SteamManager.GetSteamIdAsLong());
        
        if (_userInfo is null)
        {
            CSteamID playerId = new CSteamID(SteamManager.GetSteamIdAsLong());
            
            _userInfo = new Users
            {
                SteamID = playerId.m_SteamID,
                LobbyID = 0,
                Nickname = SteamFriends.GetFriendPersonaName(playerId),
                AvatarURL = SteamFriends.GetLargeFriendAvatar(playerId)
            };

            await dbContext.Users.AddAsync(_userInfo);
            await dbContext.SaveChangesAsync();
        }
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