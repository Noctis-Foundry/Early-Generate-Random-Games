using System.Collections.Generic;
using System.Threading.Tasks;
using GameRandom.Src.SteamsContexts;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public interface ILobbyUpdate
{
    public List<ProfileContext> UserContext { get; }
    public Task UpdateLobby();
    public void Dispose();
}