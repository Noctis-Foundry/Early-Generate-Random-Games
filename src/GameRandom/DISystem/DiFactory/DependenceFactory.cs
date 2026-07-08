using System;

namespace GameRandom.DISystem.DiFactory;

public class DependenceFactory : IFactoryDependence
{
    public TInstanceType CreateInstance<TInstanceType>() where TInstanceType : class, new()
    {
        
        
        return new TInstanceType();
    }

    public TInstanceType FactoryInstance<TInstanceType>(Func<TInstanceType> factory)
    {
        return factory.Invoke();
    }
}