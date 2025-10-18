using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class RollGameViewModel : ViewModelBase
{
    public async Task ChooseGame(AppSavedContext savedContext)
    {
        var instance = SteamManager.GetSteamManager();
        
        await using (var db = new AppDbContext())
        {
            
        }
    }
}