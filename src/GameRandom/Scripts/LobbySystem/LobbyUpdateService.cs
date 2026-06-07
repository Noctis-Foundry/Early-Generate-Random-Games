using System;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Src;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.UserData;

namespace GameRandom.Scripts.LobbySystem;

public class LobbyUpdateService : IDisposable
{
    [Inject] private DatabaseService _databaseService = null!;
    [Inject] private LobbyRegister _lobbyRegister = null!;
    [Inject] private SteamService _steamService = null!;
    [Inject] private ISteamWebService _steamWebService = null!;
    [Inject] private IRouteManager _routeManager = null!;
    
   

    public LobbyUpdateService()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);
        ValidationDependence();
        
        _routeManager.GetRouteService(TableEnum.Lobby).Subscribe(RouteStage.Data, UpdateLobbyRegister);
    }
    
    public async Task UpdateLobbyRegister()
    {
        var lobbyId = User.GetInstance().GetUserInfo().LobbyId;
        
        var lobbyMembers = await _databaseService.GetLobbyById(lobbyId);

        if (lobbyMembers is null || lobbyMembers.LobbyData.Count <= 0)
            return;

        foreach (var memberInfo in lobbyMembers.LobbyData)
        {
            var userInfo = await _databaseService.GetUserByUlongId(memberInfo.UserId);
            if (userInfo is null) continue;
            
            var userProfile = await _steamWebService.GetProfile(memberInfo.UserId);
            if (userProfile is null) continue;
            
            var avatarByts = await _steamService.GetImage(userProfile.avatarUrl);
            if (avatarByts is null) continue;
            
            var nickname = string.IsNullOrEmpty(userInfo.Nickname) ? "Unknown" : userInfo.Nickname;

            var lobbyMemberInfo = new LobbyUserInfo(userInfo.SteamId, nickname, avatarByts);
            _lobbyRegister.RegisterUser(userInfo.SteamId, lobbyMemberInfo);
        }
    }

    private void ValidationDependence()
    {
        if (_databaseService is null)
            throw new ArgumentNullException(nameof(_databaseService));
        
        if (_steamWebService is null)
            throw new ArgumentNullException(nameof(_steamWebService));
        
        if (_lobbyRegister is null)
            throw new ArgumentNullException(nameof(_lobbyRegister));
        
        if (_routeManager is null)
            throw new ArgumentNullException(nameof(_routeManager));

        if (_steamService is null)
            throw new ArgumentNullException(nameof(_steamService));
    }

    public void Dispose()
    {
        _databaseService = null!;
        _steamWebService = null!;
        _lobbyRegister = null!;
        _steamService = null!;
    }
}