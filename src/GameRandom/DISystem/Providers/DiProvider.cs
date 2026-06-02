using GameRandom.DependenceInjectSystem.DiInterfaces;
using GameRandom.DependenceInjectSystem.DiSystem;

namespace GameRandom.DependenceInjectSystem.Providers;

/// <summary>
/// For usage this provide, override BindingInstance
/// </summary>
public abstract class DiProvider
{
    protected readonly IBindingDiContainer DiContainer = Di.BindingInstance;

    public virtual void BindingInstance()
    {
        
    }
}