using System.Threading.Tasks;

namespace GameRandom.ViewModels.CurrentGameSystem.Interface;

public interface ICurrentGameLoad
{
    public Task<CurrentGameLoadData?> LoadInfo();
    public void Dispose();
}