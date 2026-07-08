using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.Scripts.RollGameSystem;
using GameRandom.Scripts.SteamSDK.SteamsContexts;

namespace GameRandom.Scripts.SteamSDK.SteamWebAPI;

public interface ISteamWebService
{
    public Task<ProfileContext?> GetProfile(ulong steamId64);
    public Task<JsonDocument?> GetOwnedGames(ulong steamId64);
    public Task<GenerateGameStruct> GetGameFromStore(int appId);
}