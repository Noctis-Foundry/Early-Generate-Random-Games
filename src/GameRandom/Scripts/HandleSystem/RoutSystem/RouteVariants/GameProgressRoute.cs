using System;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DbContext;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.Service;

namespace GameRandom.Scripts.HandleSystem.RoutSystem.RouteVariants;

public class GameProgressRoute : RouteService
{
    public override async Task Route(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.GameProgress)
        {
            Logger.Warning("Is not admin payload structure");
            return;
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var adminData = await _databaseService.GetFromRowId<GameProgresses>(payloadStructure.RowId, cts.Token);

        if (adminData == null)
            return;

        await BaseHandle(payloadStructure);
    }

    public override void SendEvent(object? data = null)
    {
        _eventBus.Publish(new GameProgressEvent());
    }
}