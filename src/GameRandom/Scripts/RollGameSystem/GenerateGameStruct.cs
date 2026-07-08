using GameRandom.Scripts.RollGameSystem.Enums;
using GameRandom.Scripts.RollGameSystem.GenerateGames;

namespace GameRandom.Scripts.RollGameSystem;

public struct GenerateGameStruct
{
    public GenerationStatusCode StatusCode;
    public AppSavedContext? AppSavedContext;
}