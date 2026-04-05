using System;
using GameRandom.DependenceInjectSystem.Enums;

namespace GameRandom.DependenceInjectSystem.Binders;

public class BindingInfo
{
    public Type? ContractType;
    public Type? ImplementationType;
    public bool IsLazy;
    public ScopeType ScopeType;
}

public class BindingInstanceInfo
{
    public Type? ContractType;
    public object? Instance;
}