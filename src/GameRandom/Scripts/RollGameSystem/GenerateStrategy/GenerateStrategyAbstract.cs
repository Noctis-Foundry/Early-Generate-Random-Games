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
using GameRandom.Src.UserData;

namespace GameRandom.Scripts.RollGameSystem.GenerateStrategy;

public abstract class GenerateStrategy : IDisposable
{
    protected HashSet<int> IndexSet = new HashSet<int>();

    protected const int MinYear = 2010;
    protected const int MaxIter = 1000;

    protected int ElementIndex = 0;
    
    public abstract Task<AppSavedContext?> GenerateGame<T>(JsonDocument document, T? inputData1);
    
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

public class GenerateRandomGame : GenerateStrategy
{
    public override async Task<AppSavedContext?> GenerateGame<T>(JsonDocument document, 
        T? inputData = default) where T : default
    {
        var rootElement = document.RootElement;
        var arrayLenght = rootElement.GetArrayLength();
        
        var isIndexUpdated = CalculateArrayIndex(arrayLenght);

        if (isIndexUpdated)
            return rootElement[ElementIndex].Deserialize<AppSavedContext>(JsonSerializerOptions());

        return null;
    }
}

public class GenerateGameByYear : GenerateStrategy
{
    public override async Task<AppSavedContext?> GenerateGame<T>(JsonDocument document,
        T? inputData) where T : default
    {
        if (inputData is not ICollection<int> year)
        {
            Logger.Error("Failed to get collection");
            return null;
        }
        
        var rootElement = document.RootElement;
        var arrayLenght = rootElement.GetArrayLength();

        for (int i = 0; i < MaxIter; i++)
        {
            ElementIndex = Random.Shared.Next(0, arrayLenght);

            if (IndexSet.Add(ElementIndex))
            {
                var element = rootElement[ElementIndex];
                var elementYear = element.GetProperty("appReleaseYear").GetInt32();
                
                if (year.Contains(elementYear))
                    return element.Deserialize<AppSavedContext>(JsonSerializerOptions());
            }
        }

        return null;
    }
}

public sealed class GenerateGameFromUserId : GenerateStrategy
{
    [Inject] private SteamWebApi _steamWebApi = null!;

    public GenerateGameFromUserId()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_steamWebApi is null)
            throw new NullReferenceException(nameof(_steamWebApi));
    }
    
    public override async Task<AppSavedContext?> GenerateGame<T>(JsonDocument document, 
        T? inputData1) where T : default
    {
        var jsonDocument = await _steamWebApi.GetOwnedGames(
            User.GetInstance().GetUserId());

        if (jsonDocument is null)
            return null;
        
        var rootElement = jsonDocument.RootElement.GetProperty("response").GetProperty("games");
        var arrayLenght = rootElement.GetArrayLength();
        
        var arrayIndex = Random.Shared.Next(0, arrayLenght);
        var currentElement = rootElement[arrayIndex];
        
        var appId = currentElement.GetProperty("appid").GetInt32();

        var game = document.RootElement;
        var currentGameFromAppId = game.EnumerateArray().FirstOrDefault(e => 
            e.GetProperty("appId").GetInt32() == appId);

        return currentGameFromAppId.Deserialize<AppSavedContext>(JsonSerializerOptions());
    }

    public override void Dispose()
    {
        _steamWebApi = null!;
        base.Dispose();
    }
}