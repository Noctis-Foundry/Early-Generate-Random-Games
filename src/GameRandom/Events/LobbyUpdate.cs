using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class LobbyUpdate
{
    public List<Users>? LobbyMembers;

    public LobbyUpdate(List<Users>? lobbyMembers)
    {
        LobbyMembers = lobbyMembers;
    }
}