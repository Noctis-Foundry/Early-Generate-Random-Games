using GameRandom.Scripts.HandleSystem.Enums;

namespace GameRandom.Scripts.HandleSystem.Interfaces;

public interface IRouteManager
{
    public IRouteService GetRouteService(TableEnum tableEnum);
}