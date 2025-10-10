using System;
using System.ComponentModel;

namespace GameRandom.SteamSDK.DI;

public static class Di
{
    private static readonly DiContainer DiContainer = new DiContainer();
    public static DiContainer Container => DiContainer;
}

public class DiFactory
{
    private DiContainer _container = Di.Container;

    public void Create<TClass, T1>(TClass instance, T1 arg1) 
        where TClass : Register
    {
        instance.Init(arg1);
        _container.RegisterSingleInstance<TClass>(instance);
    }

    public void Create<TClass, T1, T2>(TClass instance, T1 arg1, T2 arg2)
        where TClass : Register
    {
        instance.Init(arg1, arg2);
        _container.RegisterSingleInstance<TClass>(instance);
    }

    public void Create<TClass>(TClass instance, params object[] args) 
        where TClass : Register
    {
        var initMethod = typeof(TClass).GetMethod("Init");
        if  (initMethod == null)
            throw new Exception($"Can't find Init in class {typeof(TClass).FullName})");
        
        initMethod.Invoke(instance, args);
        _container.RegisterSingleInstance<TClass>(instance);
    }
}

public abstract class Register
{
    public virtual void Init<T1>(T1 arg1)
    {
        
    }

    public virtual void Init<T1, T2>(T1 arg1, T2 arg2)
    {
        
    }

    public virtual void Init<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
    {
        
    }

    public virtual void Init<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        
    }
}