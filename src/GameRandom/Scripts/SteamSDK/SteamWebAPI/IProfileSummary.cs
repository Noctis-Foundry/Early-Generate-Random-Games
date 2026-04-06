using System.Threading.Tasks;
using GameRandom.Src.SteamsContexts;

namespace GameRandom.Src;

public interface IProfileSummary
{
    public Task<ProfileContext?> GetUserData(string steamUrl, ulong userId);
}