using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class UpdateTableEvent
{
    public List<GameProgress> GameProgress;
    
    public UpdateTableEvent( List<GameProgress> gameProgress)
    {
        GameProgress = gameProgress;
    }
}