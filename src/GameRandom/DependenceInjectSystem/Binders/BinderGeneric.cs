using GameRandom.DependenceInjectSystem.DiInterfaces;

namespace GameRandom.DependenceInjectSystem.Binders;

public class BinderGeneric (IFinalizedBinding container, BindingInfo bindingInfo)
{
    private IFinalizedBinding _container = container;
    private BindingInfo _bindingInfo = bindingInfo;

    public ScopeBinder To<TContract>()
    {
        _bindingInfo.ImplementationType = typeof(TContract);
        
        return new ScopeBinder(_container, _bindingInfo);
    }
}