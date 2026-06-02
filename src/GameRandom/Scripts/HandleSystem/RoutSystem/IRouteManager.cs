using GameRandom.Src.HandleSystem.Interfaces;

namespace GameRandom.Scripts.HandleSystem.RoutSystem;

public interface IRouteManager
{
    public IRouteService GetRouteService(TableEnum tableEnum);
}