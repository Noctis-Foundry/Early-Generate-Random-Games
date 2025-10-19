using System;

namespace GameRandom.Scr.DI;

/// <summary>
/// Dependency Injection factory responsible for creating and registering instances
/// into a shared <see cref="GameRandom.Scr.DI"/>. Supports flexible initialization
/// with arguments and interface-based registration.
/// </summary>
public class DiFactory
{
    private DiContainer _container = Di.Container;

    /// <summary>
    /// Creates and registers a single instance of the specified class.
    /// Calls <see cref="GameRandom.SteamSDK.DI.Register.Init"/> with one initialize argument.
    /// </summary>
    /// <typeparam name="TClass">The concrete class type to register.</typeparam>
    /// <typeparam name="T1">The type of the first initialization argument.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="arg1">The first argument for the <c>Init</c> method.</param>
    public void Create<TClass, T1>(TClass instance, T1 arg1) 
        where TClass : Register
    {
        instance.Init(arg1);
        _container.RegisterSingleInstance<TClass>(instance);
    }

    /// <summary>
    /// Creates and registers a single instance implementing a specific interface.
    /// Calls <see cref="GameRandom.SteamSDK.DI.Register.Init"/> with one initialization argument.
    /// </summary>
    /// <typeparam name="TInterface">The interface to register the instance as.</typeparam>
    /// <typeparam name="TClass">The concrete class implementing the interface.</typeparam>
    /// <typeparam name="T1">The type of the first initialization argument.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="arg1">The first argument for the <c>Init</c> method.</param>
    public void Create<TInterface, TClass, T1>(TClass instance, T1 arg1)
        where TClass : Register, TInterface
    {
        instance.Init(arg1);
        _container.RegisterSingleInstance<TInterface>(instance);
    }

    /// <summary>
    /// Creates and registers a single instance of the specified class.
    /// Calls <see cref="GameRandom.SteamSDK.DI.Register.Init"/> with two initialization arguments.
    /// </summary>
    /// <typeparam name="TClass">The concrete class type to register.</typeparam>
    /// <typeparam name="T1">The type of the first initialization argument.</typeparam>
    /// <typeparam name="T2">The type of the second initialization argument.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="arg1">The first argument for the <c>Init</c> method.</param>
    /// <param name="arg2">The second argument for the <c>Init</c> method.</param>
    public void Create<TClass, T1, T2>(TClass instance, T1 arg1, T2 arg2)
        where TClass : Register
    {
        instance.Init(arg1, arg2);
        _container.RegisterSingleInstance<TClass>(instance);
    }

    /// <summary>
    /// Creates and registers a single instance implementing a specific interface.
    /// Calls <see cref="GameRandom.SteamSDK.DI.Register.Init"/> with two initialization arguments.
    /// </summary>
    /// <typeparam name="TInterface">The interface to register the instance as.</typeparam>
    /// <typeparam name="TClass">The concrete class implementing the interface.</typeparam>
    /// <typeparam name="T1">The type of the first initialization argument.</typeparam>
    /// <typeparam name="T2">The type of the second initialization argument.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="arg1">The first argument for the <c>Init</c> method.</param>
    /// <param name="arg2">The second argument for the <c>Init</c> method.</param>
    public void Create<TInterface, TClass, T1, T2>(TClass instance, T1 arg1, T2 arg2)
        where TClass : Register, TInterface
    {
        instance.Init(arg1, arg2);
        _container.RegisterSingleInstance<TInterface>(instance);
    }

    /// <summary>
    /// Creates and registers a single instance of the specified class.
    /// Uses reflection to call the <c>Init</c> method with any number of arguments.
    /// </summary>
    /// <typeparam name="TClass">The concrete class type to register.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="args">An array of arguments to pass to <c>Init</c>.</param>
    /// <exception cref="Exception">Thrown if the <c>Init</c> method cannot be found.</exception>
    public void CreateDynamic<TClass>(TClass instance, params object[] args) 
        where TClass : Register
    {
        var initMethod = typeof(TClass).GetMethod("Init");
        if (initMethod == null)
            throw new Exception($"Can't find Init in class {typeof(TClass).FullName})");
        
        initMethod.Invoke(instance, args);
        _container.RegisterSingleInstance(instance);
    }

    /// <summary>
    /// Creates and registers a single instance implementing a specific interface.
    /// Uses reflection to call the <c>Init</c> method with any number of arguments.
    /// </summary>
    /// <typeparam name="TInterface">The interface to register the instance as.</typeparam>
    /// <typeparam name="TClass">The concrete class implementing the interface.</typeparam>
    /// <param name="instance">The instance to initialize and register.</param>
    /// <param name="args">An array of arguments to pass to <c>Init</c>.</param>
    /// <exception cref="Exception">Thrown if the <c>Init</c> method cannot be found.</exception>
    public void CreateDynamic<TInterface, TClass>(TClass instance, params object[] args) 
        where TClass : Register, TInterface
    {
        var initMethod = typeof(TClass).GetMethod("Init");
        if (initMethod == null)
            throw new Exception($"Can't find Init in class {typeof(TClass).FullName})");
        
        initMethod.Invoke(instance, args);
        _container.RegisterSingleInstance<TInterface>(instance);
    }
}
