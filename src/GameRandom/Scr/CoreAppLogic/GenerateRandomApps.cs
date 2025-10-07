using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.Service;
using GameRandom.SteamSDK;

namespace GameRandom.CoreApp;

public class GenerateRandomApps : IGenApp
{
    private Dictionary<int, List<AppSavedContext>> _apps = new();
    private string _localPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "temp_apps.json");
    private readonly Random _rng = new();

    public bool IsInitialized { get; private set; } = false;
    
    public GenerateRandomApps()
    {
        Console.WriteLine($"Path to json file: {_localPath}");
        
        if (!File.Exists(_localPath))   
        {
            throw new FileNotFoundException("The apps file was not found.");
        }
        
        GetAppList();
        
        IsInitialized = true;
    }

    private void GetAppList()
    {
        string json = File.ReadAllText(_localPath);
        
        if (string.IsNullOrEmpty(json))
            throw new FileNotFoundException("The apps.json file was not found.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        var apps = JsonSerializer.Deserialize<List<AppSavedContext>>(json, options);
        
        if (apps == null)
            throw new FileNotFoundException("The apps saved context was not found.");
        
        try
        {
            foreach (var app in apps)
            {
                if (_apps.ContainsKey(app.AppReleaseYear))
                    _apps[app.AppReleaseYear].Add(app);
                else
                {
                    _apps.Add(app.AppReleaseYear, new List<AppSavedContext>());
                    _apps[app.AppReleaseYear].Add(app);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception("Problems with convert json file to dictionary: " + e.Message);
        }
    }
    public AppSavedContext? GetRandomGame(int year)
    {
        if (!_apps.ContainsKey(year))
            return null;
        
        if (_apps.TryGetValue(year, out var apps))
        {
            return apps[_rng.Next(0, apps.Count)];
        }

        return null;
    }
    public AppSavedContext? GetRandomGame(int year, int indexCategory)
    {
        if (_apps.TryGetValue(year, out var apps))
        {
            AppSavedContext? app = apps.Find(a => a.AppCategoris.ContainsKey(indexCategory));
            return app;
        }

        return null;
    }
}

public interface IGenApp
{
    bool IsInitialized { get; }
    
    AppSavedContext? GetRandomGame(int year);
    AppSavedContext? GetRandomGame(int year, int indexCategory);
}

