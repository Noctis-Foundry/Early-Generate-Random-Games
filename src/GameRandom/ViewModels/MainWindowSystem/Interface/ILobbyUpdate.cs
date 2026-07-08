using System.Collections.Generic;
using System.Threading.Tasks;
using GameRandom.Scripts.SteamSDK.SteamsContexts;

namespace GameRandom.ViewModels.MainWindowSystem.Interface;

public interface ILobbyUpdate
{
    public List<ProfileContext> UserContext { get; }
    public Task UpdateLobby();
    public void Dispose();
}