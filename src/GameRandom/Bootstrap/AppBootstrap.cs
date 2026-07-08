using System.Threading.Tasks;
using GameRandom.Providers;
using GameRandom.Scripts.StartupLogic;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Scripts.UserData;

namespace GameRandom.Bootstrap;

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