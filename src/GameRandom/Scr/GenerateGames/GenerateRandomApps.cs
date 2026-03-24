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
    private string _localPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "temp_apps.json");
    private List<AppSavedContext>? _apps = new();
    private readonly Random _rng = new();

    public bool IsInitialized { get; private set; } = false;
    
    public GenerateRandomApps(string? localPath = null)
    {
        if (localPath != null)
        {
            _localPath = localPath;
        }

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
        
        _apps = JsonSerializer.Deserialize<List<AppSavedContext>>(json, options);
        
        if (_apps == null)
            throw new FileNotFoundException("The apps saved context was not found.");
    }
    public AppSavedContext? GetRandomGame(int year)
    {
        if (_apps is null) throw new ArgumentNullException(nameof(_apps));
        
        var listYear = _apps.Where(e => e.AppReleaseYear == year).ToList();

        if (listYear.Count == 0)
            return null;
        
        return listYear[Random.Shared.Next(0, listYear.Count)];
    }

    public AppSavedContext? GetRandomGame()
    {
        if (_apps is null) throw new ArgumentNullException(nameof(_apps));
        
        return _apps[Random.Shared.Next(0, _apps.Count)];
    }
}

public interface IGenApp
{
    bool IsInitialized { get; }
    
    AppSavedContext? GetRandomGame(int year);
    
    AppSavedContext? GetRandomGame();
}

