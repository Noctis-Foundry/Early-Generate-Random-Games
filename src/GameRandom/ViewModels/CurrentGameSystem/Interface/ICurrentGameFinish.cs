using System.Threading.Tasks;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.CurrentGameSystem.Interface;

public interface ICurrentGameFinish
{
  
    public Task<UserGame> FinishingGame(GameProgresses gameInfo);
    public void Dispose();
}
