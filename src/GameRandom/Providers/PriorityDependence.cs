using GameRandom.DependenceInjectSystem.Enums;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Src;

namespace GameRandom.Providers;

public sealed class PriorityDependence : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.FromInstance<DatabaseService>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.Bind<IRouteManager>().To<RouteManager>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<DatabaseTransitionService>().ScopeBind(ScopeType.Singleton);
        DiContainer.Bind<ISteamWebService>().To<SteamWebApi>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<SteamService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<EventBus>().ScopeBind(ScopeType.Singleton);
    }
}