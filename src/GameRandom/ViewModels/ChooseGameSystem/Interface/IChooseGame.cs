using System.Threading.Tasks;
using GameRandom.CoreApp;

namespace GameRandom.ViewModels.ChooseGameSystem.Interface;

public interface IChooseGame
{
    public Task<bool> ChooseGame(AppInfo appInfo);
    public void Dispose();
}
