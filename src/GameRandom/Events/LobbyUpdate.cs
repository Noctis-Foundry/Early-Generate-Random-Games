using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class LobbyUpdate
{
    public List<LobbyUserContext>? LobbyMembers;

    public LobbyUpdate(List<LobbyUserContext>? lobbyMembers)
    {
        LobbyMembers = lobbyMembers;
    }
}