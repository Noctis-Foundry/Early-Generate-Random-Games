using GameRandom.DISystem.Enums;
using GameRandom.DISystem.Providers;
using GameRandom.Scripts.Database;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Scripts.LobbySystem;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Scripts.SteamSDK.SteamWebAPI;

namespace GameRandom.Providers;

public sealed class PriorityDependence : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.FromInstance<DatabaseService>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<DatabaseTransitionService>().ScopeBind(ScopeType.Singleton);
        DiContainer.Bind<ISteamWebService>().To<SteamWebApi>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<SteamService>().ScopeBind(ScopeType.Singleton);
        BindingRouteManager();
    }

    private void BindingRouteManager()
    {
        DiContainer.FromInstance<EventBus>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<LobbyRegister>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.Bind<IRouteManager>().To<RouteManager>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.Bind<LobbyService>().To<LobbyService>().ScopeBind(ScopeType.Singleton, false);
    }
}