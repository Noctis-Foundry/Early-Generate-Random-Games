using System;
using System.Collections.Generic;

namespace GameRandom.Scr.DI;

public class DependenceBase
{
    private static readonly HashSet<Type> _readyTypes = new HashSet<Type>();

    public DependenceBase()
    {
        if (_readyTypes.Contains(GetType()))
            return;
        
        _readyTypes.Add(GetType());
    }
}