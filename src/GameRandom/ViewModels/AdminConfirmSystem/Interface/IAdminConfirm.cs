using System.Threading.Tasks;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.AdminConfirmSystem.Interface;

public interface IAdminConfirm
{
    public Task<bool> RejectGame(FinishedGames finishedGames);
    public Task<bool> AcceptGame(FinishedGames finishedGames);
    public void Dispose();
}