using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Providers;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.StartupLogic;
using GameRandom.Src.UserData;

namespace GameRandom.Src;

public class AppBootstrap
{
    public async Task PriorityInitialization()
    {
        await new GameEnvLoad().InitializeEnv();

        var priorityDependence = new PriorityDependence();
        priorityDependence.BindingInstance();
            
        SteamManager.GetSteamManager().InitSteam();
        
        await User.GetInstance().InitializeUser();
        
        InitializeCoreDependencies();
    }

    private void InitializeCoreDependencies()
    {
        var appProvider = new StartAppProvider();
        appProvider.BindingInstance();
    }
}