using System;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.PostgresListener;

namespace GameRandom.Src.HandleSystem.Interfaces;

public interface IRouteService
{
    public void Route(PayloadStructure payloadStructure);
    public void Subscribe(RouteStage routeUpdateStage, Action process);
    public void Dispose();
}