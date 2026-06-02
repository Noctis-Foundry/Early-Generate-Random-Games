using System;
using System.Collections.Generic;
using GameRandom.Scr.Service;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem.RouteVariants;
using GameRandom.Src.HandleSystem.Interfaces;

namespace GameRandom.Scripts.HandleSystem.RoutSystem;

public class RouteManager : IDisposable, IRouteManager
{
    private readonly Dictionary<TableEnum, IRouteService> _routeServices = new();
    
    private PostgresListener.PostgresListener _listener = null!;
    private Action<TableEnum, PayloadStructure> _chooseRouteService;

    public void Start()
    {
        _listener = new PostgresListener.PostgresListener();
        _chooseRouteService = ChooseRouteService;
        _listener.NotificationCallback += _chooseRouteService;
        
        BindingRoutes();
    }
    
    public IRouteService GetRouteService(TableEnum tableEnum)
    {
        if (!_routeServices.TryGetValue(tableEnum, out var routeService))
        {
            Logger.Warning($"Failed to find route service for {tableEnum}");
            return null!;
        }

        return routeService;
    }
    
    private void BindingRoutes()
    {
        _routeServices.TryAdd(TableEnum.Lobby, new LobbyRoute());
    }
    private void ChooseRouteService(TableEnum tableEnum, PayloadStructure structure)
    {
        if (!_routeServices.TryGetValue(tableEnum, out var routeService))
        {
            Logger.Warning($"Failed to find route service for {tableEnum}");
            return;
        }
        
        routeService.Route(structure);
    }
    
    public void Dispose()
    {
        _listener.Dispose();

        foreach (var routeService in _routeServices.Values)
        {
            routeService.Dispose();
        }
        
        _routeServices.Clear();
    }

 
}