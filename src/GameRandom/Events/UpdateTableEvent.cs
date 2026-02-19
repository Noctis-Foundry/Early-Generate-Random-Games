using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class UpdateTableEvent
{
    public List<GameProgresses> GameProgress;
    
    public UpdateTableEvent( List<GameProgresses> gameProgress)
    {
        GameProgress = gameProgress;
    }
}