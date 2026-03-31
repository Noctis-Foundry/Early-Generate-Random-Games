using System.Collections.Generic;
using System.Threading.Tasks;
using GameRandom.Src.SteamsContexts;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public interface ILobbyUpdate
{
    public Task<List<ProfileContext>> UpdateLobby();
    public void Dispose();
}