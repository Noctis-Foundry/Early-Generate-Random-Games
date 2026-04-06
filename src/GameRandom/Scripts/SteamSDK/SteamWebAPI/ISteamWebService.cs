using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.Src.RollGameSystem;
using GameRandom.Src.SteamsContexts;

namespace GameRandom.Src;

public interface ISteamWebService
{
    public Task<ProfileContext?> GetProfile(ulong steamId64);
    public Task<JsonDocument?> GetOwnedGames(ulong steamId64);
    public Task<GenerateGameStruct> GetGameFromStore(int appId);
}