using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;

namespace GameRandom.Src;

public interface IUserOwnerGames
{
    public Task<JsonDocument?> GetPlayerLibrary(string steamApiKey);
    public Task<AppSavedContext?> GetAppInfoFromAppId(int appId);
}