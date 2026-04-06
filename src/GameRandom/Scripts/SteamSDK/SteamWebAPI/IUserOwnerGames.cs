using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.Src.RollGameSystem;

namespace GameRandom.Src;

public interface IUserOwnerGames
{
    public Task<JsonDocument?> GetPlayerLibrary(string steamApiKey);
    public Task<GenerateGameStruct> GetAppInfoFromAppId(int appId);
}