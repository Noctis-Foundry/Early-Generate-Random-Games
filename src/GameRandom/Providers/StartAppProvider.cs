using GameRandom.DISystem.Enums;
using GameRandom.DISystem.Providers;
using GameRandom.Scripts.Factory;
using GameRandom.Scripts.LobbySystem;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;

namespace GameRandom.Providers;

public class StartAppProvider : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.FromInstance<ObservableConverter>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<MainWindowFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<UserControlFactory>().ScopeBind(ScopeType.Singleton);
        DiContainer.FromInstance<ImageConfirmService>().ScopeBind(ScopeType.Singleton);
    }
}