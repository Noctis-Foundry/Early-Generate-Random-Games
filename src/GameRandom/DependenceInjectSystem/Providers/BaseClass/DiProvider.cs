using DIContainer.DiSystem;
using GameRandom.DependenceInjectSystem.DiInterfaces;
using GameRandom.DependenceInjectSystem.DiSystem;

namespace DIContainer.Providers;

/// <summary>
/// For usage this provide, override BindingInstance
/// </summary>
public abstract class DiProvider
{
    protected readonly IBindingDiContainer DiContainer = Di.GetBindingInstance();

    public virtual void BindingInstance()
    {
        
    }
}