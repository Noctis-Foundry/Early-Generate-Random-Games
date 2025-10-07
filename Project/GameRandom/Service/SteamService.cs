using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace GameRandom.SteamSDK;

public class SteamService
{
    public static SteamService Instance { get; private set; } = new SteamService();

    public async Task<Bitmap?> GetImage(string imageUrl)
    {
        using var imageClient = new HttpClient();

        try
        {
            var response = await imageClient.GetAsync(imageUrl);

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            
            using (var mr = new MemoryStream(imageBytes))
            {
                return new Bitmap(mr);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}