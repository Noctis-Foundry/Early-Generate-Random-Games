using System;
using System.Collections.Generic;

namespace GameRandom.Scr.DI;

public class DependenceBase
{
    private static HashSet<Type> _createdTypes = new HashSet<Type>();

    public DependenceBase()
    {
        var type = GetType();
        if (_createdTypes.Contains(type))
            throw new InvalidOperationException($"Instance of {type.Name} already exists.");

        _createdTypes.Add(type);
    }
}