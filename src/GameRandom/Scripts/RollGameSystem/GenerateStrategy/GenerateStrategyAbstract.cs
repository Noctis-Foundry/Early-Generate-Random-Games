using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Src;
using GameRandom.Src.RollGameSystem;
using GameRandom.Src.UserData;

namespace GameRandom.Scripts.RollGameSystem.GenerateStrategy;

public abstract class GenerateStrategyAbstract : IDisposable
{
    protected HashSet<int> IndexSet = new HashSet<int>();

    protected const int MinYear = 2010;
    protected const int MaxIter = 1000;

    protected int ElementIndex = 0;
    
    public abstract Task<GenerateGameStruct> GenerateGame(JsonDocument document, object? inputData = null);
    
    protected JsonSerializerOptions JsonSerializerOptions()
    {
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };

        return options;
    }

    protected bool CalculateArrayIndex(int arrayLenght)
    {
        for (int i = 0; i < MaxIter; i++)
        {
            ElementIndex = Random.Shared.Next(0, arrayLenght);

            if (IndexSet.Add(ElementIndex))
                return true;
        }

        return false;
    }

    protected GenerateGameStruct ReturnSuccessesCode(AppSavedContext? appInfo)
    {
        return new GenerateGameStruct { AppSavedContext = appInfo, StatusCode = GenerationStatusCode.Successes};
    }
    
    protected GenerateGameStruct ReturnFailedCode(GenerationStatusCode code)
    {
        return new GenerateGameStruct{AppSavedContext = null, StatusCode = code};
    }
    
    public void ClearAfterGeneration()
    {
        ElementIndex = 0;
    }

    public virtual void Dispose()
    {
        ClearAfterGeneration();
        IndexSet.Clear();
    }
}

public class GenerateRandomGame : GenerateStrategyAbstract
{
    public override async Task<GenerateGameStruct> GenerateGame(JsonDocument document, 
        object? inputData = null)
    {
        var rootElement = document.RootElement;
        var arrayLenght = rootElement.GetArrayLength();
        
        var isIndexUpdated = CalculateArrayIndex(arrayLenght);

        if (isIndexUpdated)
            return ReturnSuccessesCode(rootElement[ElementIndex].Deserialize<AppSavedContext>(JsonSerializerOptions()));

        return ReturnFailedCode(GenerationStatusCode.GenerateNext);
    }
}

public sealed class GenerateGameFromUserLib : GenerateStrategyAbstract
{
    [Inject] private ISteamWebService _steamWebApi = null!;
    
    private readonly HashSet<string> _nonGameTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Photo editing",
        "Utilities",
        "Game development",
        "Animation & Modeling",
        "Illustration"
    };

    public GenerateGameFromUserLib()
    {
        Di.ResolveInstance.ResolveFiled(out _steamWebApi);

        if (_steamWebApi is null)
            throw new NullReferenceException(nameof(_steamWebApi));
    }
    
    public override async Task<GenerateGameStruct> GenerateGame(JsonDocument document, 
        object? inputData = null)
    {
        var jsonDocument = await _steamWebApi.GetOwnedGames(
            User.GetInstance().GetUserId());

        if (jsonDocument is null) 
            return ReturnFailedCode(GenerationStatusCode.Exit);
        
        var rootElement = jsonDocument.RootElement.GetProperty("response").GetProperty("games");
        var arrayLenght = rootElement.GetArrayLength();
        
        var arrayIndex = Random.Shared.Next(0, arrayLenght);
        var currentElement = rootElement[arrayIndex];
        
        var appId = currentElement.GetProperty("appid").GetInt32();

        var game = document.RootElement;
        var currentGameFromAppId = game.EnumerateArray().FirstOrDefault(e => 
            e.GetProperty("appId").GetInt32() == appId);

        if (currentGameFromAppId.ValueKind == JsonValueKind.Undefined)
            return await _steamWebApi.GetGameFromStore(appId);

        var app = currentGameFromAppId.Deserialize<AppSavedContext>(JsonSerializerOptions());

        if (app is not null && CheckGameInNonGameTypes(app))
            return new GenerateGameStruct { StatusCode = GenerationStatusCode.Successes, AppSavedContext = app};
        
        return ReturnFailedCode(GenerationStatusCode.GenerateNext);
    }

    private bool CheckGameInNonGameTypes(AppSavedContext app)
    {
        foreach (var genre in app.AppGenres)
        {
            if (_nonGameTypes.Contains(genre))
                return false;
        }

        return true;
    }
    
    public override void Dispose()
    {
        _steamWebApi = null!;
        base.Dispose();
    }
}