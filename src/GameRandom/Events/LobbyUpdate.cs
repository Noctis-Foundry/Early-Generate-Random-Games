using System.Collections.Generic;
using GameRandom.DataBaseContexts;

namespace GameRandom.Events;

public class LobbyUpdate
{
    public List<User>? LobbyMembers;

    public LobbyUpdate(List<User>? lobbyMembers)
    {
        LobbyMembers = lobbyMembers;
    }
}