using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace GameRandom.SteamSDK;

public class SteamService
{
    public static SteamService Instance { get; private set; } = new SteamService();

    public async Task<Bitmap?> GetImage(string imageUrl)
    {
        return new Bitmap(imageUrl);
    }
}