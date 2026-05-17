using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.RollGameSystem;
using GameRandom.Src.UserData;

namespace GameRandom.Scripts.RollGameSystem.GenerateStrategy;

public sealed class GenerateGameFromUserLib : GenerateStrategyAbstract
{
    [Inject] private ISteamWebService _steamWebApi = null!;

    private List<int> _userLibAppId = new();
    
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

    private async Task<GenerateGameStruct> GenerateGamesFromWeb(JsonDocument document)
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

    private async Task<GenerateGameStruct> GenerateGameFromVdfList()
    {
        if (_userLibAppId.Count <= 0)
            return ReturnFailedCode(GenerationStatusCode.Exit);
        
        var idIndex = Random.Shared.Next(0, _userLibAppId.Count);
        
        Logger.Info($"Random id = {idIndex}");

        var game = await _steamWebApi.GetGameFromStore(_userLibAppId[idIndex]);

        Logger.Debug($"Game rolling with status code {game.StatusCode} and game info == null {game.AppSavedContext == null}");
        
        return game;
    }
    
    public override async Task<GenerateGameStruct> GenerateGame(JsonDocument document, 
        object? inputData = null)
    {
        if (inputData is List<int> list)
        {
            _userLibAppId = list;
            var result = await GenerateGameFromVdfList();

            if (result.StatusCode is GenerationStatusCode.Successes)
                return result;
        }

        return await GenerateGamesFromWeb(document);
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