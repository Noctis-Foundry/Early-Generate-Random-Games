using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.Scr.Service;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Src.RollGameSystem;

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