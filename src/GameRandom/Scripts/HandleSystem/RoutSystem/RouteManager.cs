using System;
using System.Collections.Generic;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem.RouteVariants;
using GameRandom.Scripts.Service;

namespace GameRandom.Scripts.HandleSystem.RoutSystem;

public class RouteManager : IDisposable, IRouteManager
{
    private readonly Dictionary<TableEnum, RouteService> _routeServices = new();
    
    private PostgresListener.PostgresListener _listener = null!;
    private Action<TableEnum, PayloadStructure>? _chooseRouteService;

    public RouteManager()
    {
        Start();
    }
    
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
        _routeServices.TryAdd(TableEnum.UserGames, new UserGameRoute());
        _routeServices.TryAdd(TableEnum.AdminTable, new AdminRoute());
        _routeServices.TryAdd(TableEnum.GameProgress, new GameProgressRoute());
        _routeServices.TryAdd(TableEnum.FinishedGames, new FinishedGameRoute());
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