using System.Threading.Tasks;
using GameRandom.Scripts.SteamSDK.SteamsContexts;

namespace GameRandom.Scripts.SteamSDK.SteamWebAPI;

public interface IProfileSummary
{
    public Task<ProfileContext?> GetUserData(string steamUrl, ulong userId);
}