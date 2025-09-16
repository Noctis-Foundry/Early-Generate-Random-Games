using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.SteamSDK;
using Path = System.IO.Path;

namespace GameRandom.CoreApp;

public class GenerateRandomApps
{
    private readonly Random _random = new();
    
    public async Task<List<AppContext>> GenerateApps(int count)
    {
        SteamManager steamManager = SteamManager.Instance;

        if (steamManager == null)
        {
            Console.WriteLine("SteamManager dont initialize. Await app initialize");
            return new List<AppContext>();
        }

        string jsonFile = await steamManager.GetAppList();

        if (jsonFile == " ")
        {
            Console.WriteLine("No app list found. Await app list");
            return new List<AppContext>();
        }

        List<AppContext> appContexts = new List<AppContext>();

        var jsonDoc = JsonDocument.Parse(jsonFile);
        var root = jsonDoc.RootElement.GetProperty("applist").GetProperty("apps");

        var rootArray = root.EnumerateArray().ToArray();

        int iterCount = 0;

        while (iterCount < count)
        {
            if (appContexts.Count >= count)
                break;
            
            var element = rootArray[_random.Next(0, rootArray.Length)];
            
            (int appId, string nameApp) = ConvertApp(element);

            if (!await CheckImageOnApp(appId))
            {
                continue;
            }
            
            appContexts.Add(new AppContext(appId, nameApp));
            
            iterCount++;
        }
        
        return appContexts;
    }

    public async Task<Bitmap?> GetAppImage(int appId)
    {
        using var client = new HttpClient();
        string url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg";

        string localPath = Path.Combine("Images", $"{appId}.jpg");
        Directory.CreateDirectory("Images");

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Image not found for AppID {appId}, status: {response.StatusCode}");
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(localPath, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download image for {appId}: {ex.Message}");
            return null;
        }

        var bitmap = new Bitmap(localPath);
        return bitmap;
    }

    private async Task<bool> CheckImageOnApp(int appId)
    {
        using var client = new HttpClient();
        string url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg";
        
        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Image not found for AppID {appId}, status: {response.StatusCode}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    private (int, string) ConvertApp(JsonElement app)
    {
        int appId = app.GetProperty("appid").GetInt32();
        string appName = app.GetProperty("name").GetString() ?? "Unknown";

        if (appId == 0 || appName == "Unknown")
        {
            Console.WriteLine("Dont find app");
            return (0, "Unknown");
        }

        return (appId, appName);
    }
}

public class AppContext
{
    public int AppId { get; private set; }
    public string AppName { get; private set; }

    public AppContext(int appId, string appName)
    {
        AppId = appId;
        AppName = appName;
    }
}