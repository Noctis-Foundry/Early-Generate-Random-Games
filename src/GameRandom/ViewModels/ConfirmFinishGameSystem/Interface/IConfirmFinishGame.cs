using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.ConfirmFinishGameSystem.Interface;

public interface IConfirmFinishGame
{
    public Task<bool> SaveEditAsync(GameProgresses gameInfo, string comment, Bitmap image);
    public void Dispose();
}
