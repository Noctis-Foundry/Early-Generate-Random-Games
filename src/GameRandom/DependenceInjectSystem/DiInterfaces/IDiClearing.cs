using System;

namespace GameRandom.DependenceInjectSystem.DiInterfaces;

public interface IDiClearing
{
    public void UnsubscribeAll();
    public void ClearAll();
    public void UnsubscribeInstance(Type type);
}