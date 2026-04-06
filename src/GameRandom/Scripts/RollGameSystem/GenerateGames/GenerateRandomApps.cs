using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;

namespace GameRandom.Scripts.RollGameSystem.GenerateGames;

public class GenerateRandomApps : IGenApp, IDisposable
{
    private string _localPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "temp_apps.json");
    private JsonDocument _document = null!;

    private bool _jsonDocumentIsSerialize = false;
    private const int MinYear = 2010;
    private const int MaxIter = 1000;

    private HashSet<int> _pickedIndex = new();
    
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
        
        Di.ResolveInstance.ResolveInstanceFromClass(this);
        
        // Dispatcher.UIThread.Post(GetAppList); TODO Maybe deleted, if other method for generation work is very lazy
    }

    private async Task GetAppList()
    {
        await using var fs = File.OpenRead(_localPath);
        
        _document = await JsonDocument.ParseAsync(fs);

        _jsonDocumentIsSerialize = true;
    }

    public async Task StartGenerateApp()
    {
        await GetAppList();
    }

    public void EndGeneration()
    {
        _document?.Dispose();
        _document = null!;
        _jsonDocumentIsSerialize = false;
    }
    
    public AppSavedContext? GetRandomGame(List<int> years)
    {
        CheckDocumentInArgumentException();

        var rootElement = _document.RootElement;

        int elementIndex = 0;
        bool isFind = false;

        for (int i = 0;  !isFind || i < MaxIter; i++)
        {
            var randomIndex = Random.Shared.Next(0, rootElement.GetArrayLength());

            if (!_pickedIndex.Add(randomIndex))
                continue;
            
            var releaseYear = rootElement[randomIndex].GetProperty("appReleaseYear").GetInt32();

            if (years.Contains(releaseYear))
            {
                elementIndex = randomIndex;
                isFind = true;
            }
        }

        var app = rootElement[elementIndex].Deserialize<AppSavedContext>(JsonSerializerOptions());
        
        return app;
    }

    public AppSavedContext? GetRandomGameFromUserLib(JsonDocument jsonDocument)
    {
        var rootElement = jsonDocument.RootElement.GetProperty("response").GetProperty("games");
        var arrayLenght = rootElement.GetArrayLength();
        
        var arrayIndex = Random.Shared.Next(0, arrayLenght);
        var currentElement = rootElement[arrayIndex];
        
        var appId = currentElement.GetProperty("appid").GetInt32();

        var game = _document.RootElement;
        var currentGameFromAppId = game.EnumerateArray().FirstOrDefault(e => e.GetProperty("appId").GetInt32() == appId);

        return currentGameFromAppId.Deserialize<AppSavedContext>(JsonSerializerOptions());
    }

    public AppSavedContext? GetRandomGame()
    {
        CheckDocumentInArgumentException();

        var rootElement = _document.RootElement;
        var arrayLenght = rootElement.GetArrayLength();

        var arrayIndex = Random.Shared.Next(0, arrayLenght);
        var app = rootElement[arrayIndex].Deserialize<AppSavedContext>(JsonSerializerOptions());

        return app;
    }

    private JsonSerializerOptions JsonSerializerOptions()
    {
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };

        return options;
    }
    
    private void CheckDocumentInArgumentException()
    {
        if (_document is null)
            throw new ArgumentException("Failed to start random game generation. Json document is null");
    }
    
    public bool ListIsLoad() => _jsonDocumentIsSerialize;

    public void Dispose()
    {
        EndGeneration();
        _pickedIndex.Clear();
    }
}

public interface IGenApp
{
    public bool ListIsLoad();
    
    public AppSavedContext? GetRandomGame(List<int> years);

    public AppSavedContext? GetRandomGameFromUserLib(JsonDocument jsonDocument);
    
    public AppSavedContext? GetRandomGame();

    public Task StartGenerateApp();
    public void EndGeneration();

    public void Dispose();
}

