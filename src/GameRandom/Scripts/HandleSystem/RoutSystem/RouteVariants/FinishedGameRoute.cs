using System.Threading;
using System.Threading.Tasks;
using GameRandom.DbContext;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.Service;

namespace GameRandom.Scripts.HandleSystem.RoutSystem.RouteVariants;

public class FinishedGameRoute : RouteService
{
    public override async Task Route(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.FinishedGames)
        {
            Logger.Warning("Is not finished games payload structure");
            return;
        }
        
        var cts = new CancellationTokenSource(); // Create a cancellation token source
        
        var gameData = await _databaseService.GetFromRowId<FinishedGames>(payloadStructure.RowId, cts.Token);

        if (gameData == null)
            return;

        await BaseHandle(payloadStructure);
    }

    public override void SendEvent(object? data = null)
    {
        _eventBus.Publish(new FinishedGameEvent());
    }
}