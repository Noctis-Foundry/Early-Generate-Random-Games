using System.Threading.Tasks;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.CurrentGameSystem.Interface;

public interface ICurrentGameLoad
{
    public Task<CurrentGameLoadData> LoadInfo();
    public void Dispose();
}