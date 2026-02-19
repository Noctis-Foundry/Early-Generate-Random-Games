using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class LobbyUpdate
{
    public List<LobbyData>? LobbyMembers;

    public LobbyUpdate(List<LobbyData>? lobbyMembers)
    {
        LobbyMembers = lobbyMembers;
    }
}