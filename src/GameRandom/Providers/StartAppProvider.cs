using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Enums;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scripts.LobbySystem;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Factory;
using GameRandom.Src.LobbySystem;

namespace GameRandom.Providers;

public class StartAppProvider : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.Bind<LobbyService>().To<LobbyService>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<LobbyRegister>().ScopeBind(ScopeType.Singleton, false);
        DiContainer.FromInstance<TaskRunner>().ScopeBind(ScopeType.Many);
        DiContainer.FromInstance<ObservableConverter>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<MainWindowFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<UserControlFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<ImageConfirmService>().ScopeBind(ScopeType.Singleton);
    }
}