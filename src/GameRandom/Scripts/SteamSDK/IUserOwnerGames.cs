using System.Text.Json;
using System.Threading.Tasks;

namespace GameRandom.Src;

public interface IUserOwnerGames
{
    public Task<JsonDocument?> GetPlayerLibrary(string steamApiKey);
}