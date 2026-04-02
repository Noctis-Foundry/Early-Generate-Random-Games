using System;
using DIContainer.DiSystem;
using GameRandom.DependenceInjectSystem.DiInterfaces;

namespace GameRandom.DependenceInjectSystem.DiSystem;

public static class Di
{
    private static Lazy<DiContainer> _instance = new Lazy<DiContainer>(() => new DiContainer());
    public static IResolveDependence ResolveInstance => _instance.Value;
    public static IBindingDiContainer BindingInstance => _instance.Value;
    
    public static IBindingDiContainer GetBindingInstance() => _instance.Value;
    public static IResolveDependence GetResolveInstance() => _instance.Value;
}