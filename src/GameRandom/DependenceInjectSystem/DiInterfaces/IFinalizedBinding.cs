using GameRandom.DependenceInjectSystem.Binders;

namespace GameRandom.DependenceInjectSystem.DiInterfaces;

public interface IFinalizedBinding
{
    public void FinalizeBinding(BindingInfo bindingInfo);
    public void FinalizeInstanceBinding(BindingInstanceInfo bindingInfo);
}