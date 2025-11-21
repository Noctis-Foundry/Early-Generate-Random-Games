using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class LobbyUpdate
{
    public List<LobbyContext>? LobbyMembers;

    public LobbyUpdate(List<LobbyContext>? lobbyMembers)
    {
        LobbyMembers = lobbyMembers;
    }
}