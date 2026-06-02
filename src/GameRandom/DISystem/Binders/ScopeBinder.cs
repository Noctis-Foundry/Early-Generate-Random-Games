using GameRandom.DependenceInjectSystem.DiInterfaces;
using GameRandom.DependenceInjectSystem.Enums;

namespace GameRandom.DependenceInjectSystem.Binders;

public class ScopeBinder(IFinalizedBinding container, BindingInfo bindingInfo)
{
    private IFinalizedBinding _container = container;
    private BindingInfo _bindingInfo = bindingInfo;

    public void ScopeBind(ScopeType scopeType, bool isLazy = true)
    {
        _bindingInfo.IsLazy = isLazy;
        _bindingInfo.ScopeType = scopeType;
        _container.FinalizeBinding(_bindingInfo);
    }
}