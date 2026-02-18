using GameRandom.Scr.DI;
using GameRandom.Scr.Service;

namespace GameRandom.ViewModels;

public class CurrentGameStatusViewModel : ViewModelBase
{
    [Inject] private DatabaseService? database = null!;

    public void LoadInfo(int appId)
    {
        
    }
}

public class GameStatusInfo()
{
    
}