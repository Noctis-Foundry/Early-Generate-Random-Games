using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Providers;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.StartupLogic;
using GameRandom.Src.UserData;

namespace GameRandom.Src;

public class PriorityStartup
{
    public async Task PriorityInitialization()
    {
        await new GameEnvLoad().InitializeEnv();
            
        SteamManager.GetSteamManager().InitSteam();
        
        await User.GetInstance().InitializeUser();
    }
}