using System.Text.Json;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.Src.RollGameSystem;

namespace GameRandom.Scripts.RollGameSystem.GenerateStrategy;

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