using GameRandom.DISystem.DiInterfaces;
using GameRandom.DISystem.DiSystem;

namespace GameRandom.DISystem.Providers;

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