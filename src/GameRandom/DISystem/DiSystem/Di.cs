using System;
using GameRandom.DISystem.DiInterfaces;

namespace GameRandom.DISystem.DiSystem;

public static class Di
{
    private static Lazy<DiContainer> _instance = new Lazy<DiContainer>(() => new DiContainer());
    public static IResolveDependence ResolveInstance => _instance.Value;
    public static IBindingDiContainer BindingInstance => _instance.Value;
    public static IDiClearing DiClearing => _instance.Value;
}