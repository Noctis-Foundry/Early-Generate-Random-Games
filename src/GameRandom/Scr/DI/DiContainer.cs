using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRandom.SteamSDK.DI;

public class DiContainer
{
    private readonly Dictionary<Type, object> _instanceService = new Dictionary<Type, object>();

    public void RegisterSingleInstance<TInterface>(TInterface instance)
    {
        if (instance == null)
        {
            Console.WriteLine($"instance '{typeof(TInterface)}' is null");
            return;
        }
        
        _instanceService.Add(typeof(TInterface), instance);
    }
    public object GetInstance(Type type)
    {
        if (!_instanceService.ContainsKey(type))
            throw new Exception($"Not founded object with type {type}");

        var instance = _instanceService[type];
        
        return instance;
    }
    public object? TryGetInstance(Type type)
    {
        if (_instanceService.TryGetValue(type, out var instance))
        {
            return instance;
        }
        
        Console.WriteLine("Not found instance");
        return null;
    }
}