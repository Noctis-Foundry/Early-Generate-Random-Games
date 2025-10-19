using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRandom.Scr.DI;

namespace GameRandom.Scr.Events;

public class EventBus
{
    private readonly Dictionary<Type, Delegate> _events = new();

    public void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var handlerDelegate))
            _events[type] = Delegate.Combine(handlerDelegate, handler);
        else
            _events[type] = handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var handlerDelegate))
        {
            var current = Delegate.Remove(handlerDelegate, handler);
            if (current == null) _events.Remove(type);
            else _events[type] = current;
        }
    }
    
    public void Publish<T>(T eventData)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var handlerDelegate))
        {
            if (handlerDelegate is Action<T> handler)
                handler.Invoke(eventData);
        }
    }
    
    public void ClearAll() => _events.Clear();
}