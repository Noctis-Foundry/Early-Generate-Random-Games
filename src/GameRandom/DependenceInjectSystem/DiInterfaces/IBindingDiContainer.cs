using System;
using GameRandom.DependenceInjectSystem.Binders;

namespace GameRandom.DependenceInjectSystem.DiInterfaces;

public interface IBindingDiContainer
{
    public BinderGeneric Bind<TContract>();
    public ScopeBinder FromInstance<TInstance>();

    public void BindSingleton(Type type, object instance);
}