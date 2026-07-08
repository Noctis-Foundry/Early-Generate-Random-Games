using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Database;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.Service;

namespace GameRandom.Scripts.HandleSystem.RoutSystem;

public abstract class RouteService : IRouteService
{
    [Inject] protected DatabaseService _databaseService = null!;
    [Inject] protected EventBus _eventBus = null!;

    private readonly Dictionary<RouteStage, List<Func<Task>>> _routeHandlers = new()
    {
        [RouteStage.Data] = [],
        [RouteStage.Logic] = [],
        [RouteStage.View] = [],
    };
    private readonly Dictionary<RouteStage, List<Func<PayloadStructure, Task>>> _routeHandlersPayload = new()
    {
        [RouteStage.Data] = [],
        [RouteStage.Logic] = [],
        [RouteStage.View] = [],
    };

    protected RouteService()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);
        
        if (_databaseService is null)
            throw new ArgumentNullException(nameof(_databaseService));
        
        if (_eventBus is null)
            throw new ArgumentNullException(nameof(_eventBus));
    }

    public abstract Task Route(PayloadStructure payloadStructure);
    
    public abstract void SendEvent(object? data = null);
    
    public virtual void Subscribe(RouteStage routeUpdateStage, Func<Task> process)
    {
        if (!_routeHandlers.ContainsKey(routeUpdateStage))
        {
            Logger.Error("RouteStage not found in routeHandlersPayload dictionary.");
            return;
        }
        
        if (_routeHandlers.TryGetValue(routeUpdateStage, out var list))
        {
            list.Add(process);
            return;
        }
        
        throw new ArgumentException("Route stage not found in routeHandlersPayload dictionary.");
        
    }
    public void Subscribe(RouteStage routeStage, Func<PayloadStructure, Task> process)
    {
        if (!_routeHandlersPayload.ContainsKey(routeStage))
        {
            Logger.Error("RouteStage not found in routeHandlersPayload dictionary.");
            return;
        }
        
        if (_routeHandlersPayload.TryGetValue(routeStage, out var list))
        {
            list.Add(process);
            return;
        }
        
        throw new ArgumentException("Route stage not found in routeHandlersPayload dictionary.");
    }
    public void Unsubscribe(RouteStage routeStage, Func<Task> process)
    {
        if (!_routeHandlers.ContainsKey(routeStage))
        {
            Logger.Error("RouteStage not found in routeHandlersPayload dictionary.");
            return;
        }
        
        _routeHandlers[routeStage].Remove(process);
    }
    public void Unsubscribe(RouteStage routeStage, Func<PayloadStructure, Task> process)
    {
        if (!_routeHandlersPayload.ContainsKey(routeStage))
        {
            Logger.Error("RouteStage not found in routeHandlersPayload dictionary.");
            return;
        }
        
        _routeHandlersPayload[routeStage].Remove(process);
    }
    
    public virtual void Dispose()
    {
        _databaseService = null!;
    }

    protected async Task BaseHandle(PayloadStructure payloadStructure)
    {
        foreach (var handler in _routeHandlers.Values)
        {
            await HandleEvent(handler);
        }

        foreach (var handler in _routeHandlersPayload.Values)
        {
            await HandleEvent(handler, payloadStructure);
        }
        
        SendEvent();
    }
    
    protected static async Task HandleEvent(List<Func<Task>> process)
    {
        foreach (var handler in process)
        {
            try
            {
                await handler();
            }
            catch (Exception e)
            {
                Logger.Error("Error while handling event: " + e.Message);
            }
        }
    }
    protected static async Task HandleEvent(List<Func<PayloadStructure, Task>> process, PayloadStructure payloadStructure)
    {
        foreach (var handler in process)
        {
            try
            {
                await handler(payloadStructure);
            }
            catch (Exception e)
            {
                Logger.Error("Error while handling event: " + e.Message);
            }
        }
    }
}