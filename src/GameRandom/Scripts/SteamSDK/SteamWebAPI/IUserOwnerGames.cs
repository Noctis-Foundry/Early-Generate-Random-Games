using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.Scripts.RollGameSystem;

namespace GameRandom.Scripts.SteamSDK.SteamWebAPI;

public interface IUserOwnerGames
{
    public Task<JsonDocument?> GetPlayerLibrary(string steamApiKey);
    public Task<GenerateGameStruct> GetAppInfoFromAppId(int appId);
}