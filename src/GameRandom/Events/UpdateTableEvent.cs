using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class UpdateTableEvent
{
    public GameProgress GameProgress;
    
    public UpdateTableEvent(GameProgress gameProgress)
    {
        GameProgress = gameProgress;
    }
}