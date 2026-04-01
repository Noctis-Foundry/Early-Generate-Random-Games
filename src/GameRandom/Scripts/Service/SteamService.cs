using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;

namespace GameRandom.Src;

public class SteamService : DependenceBase
{
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