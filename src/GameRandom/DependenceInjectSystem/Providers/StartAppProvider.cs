using DIContainer.Providers;
using GameRandom.DependenceInjectSystem.Enums;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Factory;

namespace GameRandom.DependenceInjectSystem.Providers;

public class StartAppProvider : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.FromInstance<ObservableConverter>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<DatabaseService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<DatabaseTransitionService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<MainWindowFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<SteamWebApi>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<PostgresListener>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<UserControlFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<SteamService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<ImageConfirmService>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<TaskRunner>().ScopeBind(ScopeType.Singleton);
    }
}