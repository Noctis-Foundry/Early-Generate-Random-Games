using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.Service;
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
            
            (int appId, string nameApp) = AvaloniaService.ConvertApp(element);

            AppFilterResult? appFilterResult = await CheckAppFilter(appId);

            if (appFilterResult == null || appFilterResult.IsValid == false)
            {
                Console.WriteLine("App not success");
                continue;
            }
            
            appContexts.Add(new AppContext(appId, nameApp));
            iterCount++;
        }
        
        return appContexts;
    }

    private async Task<AppFilterResult?> CheckAppFilter(int appId)
    {
        using var httpClient = new HttpClient();
        string url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l=english&cc=US\n";

        try
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("AppFilter returned error code: " + response.StatusCode);
                return null;
            }

            var appData = await ParseJsonData(response, appId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return new AppFilterResult(new AppContext(0, "unknown"), false);
    }

    private async Task<AppData?> ParseJsonData(HttpResponseMessage response, int appID)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var root = doc.RootElement.GetProperty(appID.ToString());
            if (!root.GetProperty("success").GetBoolean())
            {
                Console.WriteLine("App ID not success");
                return null;
            }
            
            var data = root.GetProperty("data");
            
            string type = data.GetProperty("type").GetString() ?? "Unknown";
            string imageUrl = data.GetProperty("header_image").GetString() ?? "Unknown";
            string price = data.GetProperty("price_overview").GetString() ?? "0";
            
            if (type != "game" || imageUrl == "Unknown")
            {
                Console.WriteLine("App not success to filter");
                return null;
            }
            
            return new AppData(type, imageUrl, price);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}

public class AppData
{
    public string ImageUrl { get; private set; }
    public string Type { get; private set; }
    public string Price { get; private set; }

    public AppData(string type, string imageUrl, string price)
    {
        ImageUrl = imageUrl;
        Type = type;
        Price = price;
    }
}

