using System;
using System.Threading.Tasks;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.PostgresListener;

namespace GameRandom.Scripts.HandleSystem.Interfaces;

public interface IRouteService
{
    public void Subscribe(RouteStage routeUpdateStage, Func<Task> process);
    public void Subscribe(RouteStage routeStage, Func<PayloadStructure, Task> process);
    public void Unsubscribe(RouteStage routeStage, Func<Task> process);
    public void Unsubscribe(RouteStage routeStage, Func<PayloadStructure, Task> process);
    public void SendEvent(object? data = null);
}