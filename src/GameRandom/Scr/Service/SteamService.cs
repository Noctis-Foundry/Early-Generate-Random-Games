using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.Scr.Service;

namespace GameRandom.SteamSDK;

public class SteamService
{
    private static Lazy<SteamService> _instance = new(() => new SteamService());
    
    public static SteamService Instance => _instance.Value;

    private SteamService()
    {
    }

    public async Task<Bitmap?> GetImage(string imageUrl, CancellationToken cancellationToken = default)
    {
        using var imageClient = new HttpClient();

        try
        {
            var response = await imageClient.GetAsync(imageUrl, cancellationToken);

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
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

    public async Task<byte[]?> GetImageBytes(string imageUrl)
    {
        using var imageClient = new HttpClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var response = await imageClient.GetAsync(imageUrl, cts.Token);
            return await response.Content.ReadAsByteArrayAsync(cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    public Bitmap? GetImageSyncFromBytes(byte[]? imageBytes)
    {
        if (imageBytes == null) return null;
        
        using var mr = new MemoryStream(imageBytes);
        
        return new Bitmap(mr);
    }
    
    public string AppSteamPage(int appId)
    {
        return $"https://store.steampowered.com/app/{appId}/?I=english";
    }
}