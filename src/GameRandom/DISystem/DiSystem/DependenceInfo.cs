using System;
using GameRandom.DISystem.Enums;

namespace GameRandom.DISystem.DiSystem;

public class DependenceInfo(Type implementationType, ScopeType scopeType = ScopeType.Singleton, bool isLazy = true)
{
    public Type ImplementationType { get; private set; } = implementationType;
    public ScopeType ScopeType { get; private set; } = scopeType;
    public bool IsLazy { get; private set; } = isLazy;
    public object? Instance { get; set; } = null;
}