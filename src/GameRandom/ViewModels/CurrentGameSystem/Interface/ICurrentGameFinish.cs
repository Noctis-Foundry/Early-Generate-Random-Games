using System.Threading.Tasks;
using GameRandom.DbContext;

namespace GameRandom.ViewModels.CurrentGameSystem.Interface;

public interface ICurrentGameFinish
{
  
    public Task<UserGame?> FinishingGame(GameProgresses gameInfo);
    public void Dispose();
}
