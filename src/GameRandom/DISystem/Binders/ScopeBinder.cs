using GameRandom.DISystem.DiInterfaces;
using GameRandom.DISystem.Enums;

namespace GameRandom.DISystem.Binders;

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