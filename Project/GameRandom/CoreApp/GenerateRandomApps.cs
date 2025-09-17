using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.Service;
using GameRandom.SteamSDK;

namespace GameRandom.CoreApp;

public class GenerateRandomApps
{
    private readonly Random _random = new();
    private readonly int _maxParallel = 2;
    
    public async Task<List<AppFilterResult>> GenerateApps(AppFilterSend filterSend)
    {
        SteamManager steamManager = SteamManager.Instance;

        if (steamManager == null)
        {
            Console.WriteLine("SteamManager dont initialize. Await app initialize");
            return new List<AppFilterResult>();
        }

        string jsonFile = await steamManager.GetAppList();

        if (jsonFile == " ")
        {
            Console.WriteLine("No app list found. Await app list");
            return new List<AppFilterResult>();
        }

        List<AppFilterResult> appFiltersData = new();

        var jsonDoc = JsonDocument.Parse(jsonFile);
        var root = jsonDoc.RootElement.GetProperty("applist").GetProperty("apps");

        var rootArray = root.EnumerateArray().ToArray();
        SemaphoreSlim throttler = new SemaphoreSlim(_maxParallel);

        while (appFiltersData.Count < filterSend.CountGenerate)
        {
            var appIdsBatch = Enumerable.Range(0, _maxParallel)
                .Select(_ => AvaloniaService.ConvertApp(rootArray[_random.Next(0, rootArray.Length)]).Item1).ToList();

            var tasks = appIdsBatch.Select(async appId =>
            {
                await throttler.WaitAsync();
                try
                {
                    return await CheckAppFilter(appId, filterSend);
                }
                finally
                {
                    throttler.Release();
                }
            }).ToList();

            while (tasks.Any())
            {
                var completed = await Task.WhenAny(tasks);
                tasks.Remove(completed);
                
                var result = await completed;

                if (result != null && result.IsValid)
                {
                    appFiltersData.Add(result);
                    Console.WriteLine($"App selected: {result.AppData.Name} ({result.AppData.Genre})");
                    
                    if (appFiltersData.Count >= filterSend.CountGenerate)
                        break;
                }
            }
        }
        
        return appFiltersData;
    }

    private async Task<AppFilterResult?> CheckAppFilter(int appId, AppFilterSend appFilterSend)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        
        string url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l=english&cc=US\n";

        try
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("AppFilter returned error code: " + response.StatusCode);
                Task.Delay(5000).Wait();
                return null;
            }

            var appData = await ParseJsonData(response, appId);

            if (appData == null || appData.Genre != appFilterSend.AppGenres)
            {
                Console.WriteLine("Parse returned null app data");
                Task.Delay(1000).Wait();
                return null;
            }

            return new AppFilterResult(appData, true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error check app filter");
            Task.Delay(1000).Wait();
            return null;
        }
        
        return new AppFilterResult(new AppData("unknown", "unknown", "unknown"), false);
    }

    private async Task<AppData?> ParseJsonData(HttpResponseMessage response, int appID)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var root = doc.RootElement.GetProperty(appID.ToString());
            
            var data = root.GetProperty("data");
            
            string type = data.GetProperty("type").GetString() ?? "Unknown";
            string name = data.GetProperty("name").GetString() ?? "Unknown";
            string imageUrl = data.GetProperty("header_image").GetString() ?? "Unknown";
            string genreDescription = "";
            
            if (data.TryGetProperty("genres", out var genres))
            {
                foreach (var genre in genres.EnumerateArray())
                {
                    genreDescription = genre.GetProperty("description").GetString() ?? "Unknown";
                }
            }
            
            Console.WriteLine($"type: {type}, name: {name}, imageUrl: {imageUrl}");
            
            if (type != "game" || imageUrl == "Unknown" || name == "Unknown")
            {
                Console.WriteLine("App not success to filter");
                return null;
            }
            
            return new AppData(type, imageUrl, name, genreDescription);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing app data: {e.Message}");
            return null;
        }
    }
}

public class AppData
{
    public string ImageUrl { get; private set; }
    public string Type { get; private set; }

    public string Name { get; private set; }
    
    public string Genre { get; private set; }

    public AppData(string type, string imageUrl,  string name, string genre = "")
    {
        ImageUrl = imageUrl;
        Type = type;
        Name = name;
        Genre = genre;
    }
}

