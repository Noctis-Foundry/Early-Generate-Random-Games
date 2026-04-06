using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Enums;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Factory;

namespace GameRandom.Providers;

public class StartAppProvider : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.Bind<ISteamWebService>().To<SteamWebApi>().ScopeBind(ScopeType.Singleton, false);
        
        DiContainer.FromInstance<EventBus>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<ObservableConverter>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<DatabaseService>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<DatabaseTransitionService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<MainWindowFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<PostgresListener>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<UserControlFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<SteamService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<ImageConfirmService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<TaskRunner>().ScopeBind(ScopeType.Singleton);
    }
}