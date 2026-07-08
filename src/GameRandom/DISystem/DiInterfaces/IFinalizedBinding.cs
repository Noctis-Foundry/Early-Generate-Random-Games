using GameRandom.DISystem.Binders;

namespace GameRandom.DISystem.DiInterfaces;

public interface IFinalizedBinding
{
    public void FinalizeBinding(BindingInfo bindingInfo);
    public void FinalizeInstanceBinding(BindingInstanceInfo bindingInfo);
}