using System.Threading.Tasks;
using GameRandom.DbContext;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserData;

namespace GameRandom.Scripts.HandleSystem.RoutSystem.RouteVariants;

public class LobbyRoute : RouteService
{
    public override async Task Route(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.Lobby)
        {
            Logger.Warning("Is not lobby payload structure");
            return;
        }

        var lobbyData = await _databaseService.GetFromRowId<LobbyData>(payloadStructure.RowId);

        if (payloadStructure.OpCode == (int)OperationsEnum.Delete)
        {
            await BaseHandle(payloadStructure);
            return;
        }
        
        if (lobbyData == null || lobbyData.LobbyId == 0)
        {
            Logger.Warning($"Lobby data is null or empty");
            return;
        }

        if (lobbyData.LobbyId != User.GetInstance().GetUserInfo().LobbyId)
        {
            Logger.Warning("User is not in lobby");
            return;
        }

        await BaseHandle(payloadStructure);
    }

    public override void SendEvent(object? data)
    {
        _eventBus.Publish(new LobbyEvent());
    }
}