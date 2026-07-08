using System;
using GameRandom.DISystem.Enums;

namespace GameRandom.DISystem.Binders;

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