using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.SteamSDK.Events;

public class UpdateTableEvent
{
    public List<GameProgress> GameProgress { get; private set; }
    
    public UpdateTableEvent(List<GameProgress> gameProgress)
    {
        GameProgress = gameProgress;
    }
}