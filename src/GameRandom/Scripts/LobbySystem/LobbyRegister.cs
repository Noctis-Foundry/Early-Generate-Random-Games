using System.Collections.Generic;
using GameRandom.Scripts.Service;

namespace GameRandom.Scripts.LobbySystem;

public class LobbyRegister
{
    private readonly Dictionary<ulong, LobbyUserInfo> _users = new();

    public void RegisterUser(ulong userId, LobbyUserInfo userInfo)
    {
        if (!_users.TryAdd(userId, userInfo))
        {
            Logger.Warning("User already registered: " + userId);
        }
    }
    
    public LobbyUserInfo? GetUserInfo(ulong userId)
    {
        TryGetUserInfo(userId, out var userInfo);
        return userInfo;
    }
    
    public bool TryGetUserInfo(ulong userId, out LobbyUserInfo? userInfo)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            userInfo = user;
            return true;
        }
        
        Logger.Error("User not found: " + userId);
        userInfo = null;
        return false;
    }

    public bool ContainsUser(ulong userid) => _users.ContainsKey(userid);
}