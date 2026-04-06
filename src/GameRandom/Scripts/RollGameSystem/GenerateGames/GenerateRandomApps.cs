using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Scripts.RollGameSystem.GenerateStrategy;
using GameRandom.Src.RollGameSystem;
using GameRandom.Src.RollGameSystem.GenerateStrategy;

namespace GameRandom.Scripts.RollGameSystem.GenerateGames;

public class GenerateRandomApps : IGenApp, IDisposable
{
    [Inject] private TaskRunner _taskRunner = new TaskRunner();
    
    private string _localPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "temp_apps.json");
    private JsonDocument _document = null!;

    private bool _jsonDocumentIsSerialize = false;

    private Dictionary<GenerationTypes, GenerateStrategyAbstract> _generateStrategy = new()
    {
        [GenerationTypes.RandomIndex] = new GenerateRandomGame(),
        [GenerationTypes.RandomFromLibrary] = new GenerateGameFromUserLib(),
    };
    
    #region InitializeRegion

    public GenerateRandomApps(string? localPath = null)
    {
        InitializeLocalPath(localPath);
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_taskRunner is null)
            throw new NullReferenceException(nameof(_taskRunner));
    }

    private void InitializeLocalPath(string? localPath)
    {
        _localPath = localPath ?? _localPath;

        if (!File.Exists(_localPath))
            throw new FileNotFoundException("The apps file was not found.");
    }

    #endregion

    #region StartGenerationArea

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

    #endregion
    
    public async Task<GenerateGameStruct> GetRandomGame(GenerationTypes generationType, object? inputData = null)
    {
        CheckDocumentInArgumentException();
        
        if (_generateStrategy.TryGetValue(generationType, out var value))
        {
            var result =
                await _taskRunner.RunT(async () => (await value.GenerateGame(_document, inputData))!);
            value.ClearAfterGeneration();
            
            return result.Value;
        }

        Logger.Error($"Not correct type {generationType}");
        return new GenerateGameStruct {AppSavedContext = null, StatusCode = GenerationStatusCode.Exit};
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

        foreach (var generateStrategyAbstract in _generateStrategy)
        {
            generateStrategyAbstract.Value.Dispose();
        }
        
        _generateStrategy.Clear();
    }
}

public interface IGenApp
{
    public bool ListIsLoad();
    public Task<GenerateGameStruct> GetRandomGame(GenerationTypes generationType, object? inputData = null);
    public Task StartGenerateApp();
    public void EndGeneration();
    public void Dispose();
}

