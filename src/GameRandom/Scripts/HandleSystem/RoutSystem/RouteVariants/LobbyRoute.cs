using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.Service;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Src.UserData;

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

        var lobbyData = await _databaseService.GetFromRowId<Lobbies>(payloadStructure.RowId);

        if (lobbyData == null|| lobbyData.LobbyId == 0)
        {
            Logger.Warning($"Lobby data from row {payloadStructure.RowId} is null or empty");
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