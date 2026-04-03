using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DIContainer.DiFactory;
using DIContainer.DiSystem.Services;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.Binders;
using GameRandom.DependenceInjectSystem.DiFactory;
using GameRandom.DependenceInjectSystem.DiInterfaces;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Enums;

namespace DIContainer.DiSystem;

public class DiContainer : IBindingDiContainer, IFinalizedBinding, IResolveDependence, IDiClearing
{
    private Dictionary<Type, DependenceInfo> _singleInstances = new();
    private Dictionary<Type, Func<object>?> _manyInstance = new();
    
    private IFactoryDependence _dependenceFactory = new DependenceFactory();
    private FinalizeBindingService _finalizeBindingService;

    public DiContainer()
    {
        _finalizeBindingService = new FinalizeBindingService(CreateInstance);
    }
    
    #region BaseBindingCommands

    public BinderGeneric Bind<TContarct>()
    {
        var bindingInfo = new BindingInfo
        {
            ContractType = typeof(TContarct)
        };

        return new BinderGeneric(this, bindingInfo);
    }
    public ScopeBinder FromInstance<TInstance>()
    {
        var type = typeof(TInstance);

        return new ScopeBinder(this, new BindingInfo
        {
            ContractType = null,
            ImplementationType = type
        });
    }

    public void BindSingleton(Type type, object instance) //TODO Change to provider binding
    {
        var dependence = new DependenceInfo(instance.GetType(), ScopeType.Singleton, false)
        {
            Instance = instance
        };

        _singleInstances[type] = dependence;
    }

    #endregion
    
    #region ResolveDependence

    public void ResolveInstanceFromClass(object classInstance)
    {
        var list = GetTypeList(classInstance.GetType());

        if (!list.IsSucesses || list.Value is null)
            throw new NullReferenceException(
                "Failed to resolve instance from class instance in Di container. Class instance is null");
        
        foreach (var field in list.Value)
        {
            if (field.GetValue(classInstance) is not null)
                continue;
            
            var dependence = FindDependence(field.FieldType);
            
            if (dependence is null)
                continue;
            
            field.SetValue(classInstance, dependence);
        }
    }

    public void ResolveFiled<TField>(out TField instance) where TField : class
    {
        var dependence = FindDependence(typeof(TField));
        
        instance = dependence as TField 
                   ?? throw new InvalidCastException($"Cannot cast {dependence.GetType()} to {typeof(TField)}");
    }

    public TType TryGetInstance<TType>() where TType : class
    {
        var instance = FindDependence(typeof(TType)) as TType;

        return instance;
    }

    #endregion
    
    #region ReflectionMethods

    private object? FindDependence(Type type)
    {
        if (_singleInstances.TryGetValue(type, out var singleInstance))
        {
            if (singleInstance.Instance is null && !type.IsInterface)
                _singleInstances[type].Instance = CreateInstance(type);
            else if (singleInstance.Instance is null && type.IsInterface)
                _singleInstances[type].Instance = CreateInstance(singleInstance.ImplementationType);

            return singleInstance.Instance; //Creating in create instance
        }

        if (_manyInstance.TryGetValue(type, out var manyInstance))
        {
            var instance = manyInstance?.Invoke();
            
            return instance; //Creating in finalizeBinding
        }

        return null;
    }
    private (bool IsSucesses, List<FieldInfo>? Value) GetTypeList(Type? classInstance)
    {
        if (classInstance == null)
            return (false, null);

        var typesList = classInstance.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(e => e.GetCustomAttributes<InjectAttribute>() != null && !e.FieldType.IsValueType).ToList();

        return (true, typesList);
    }

    #endregion

    private object CreateInstance(Type typeInstance)
    {
        var method = typeof(DependenceFactory).GetMethod(nameof(CreateInstance))!.MakeGenericMethod(typeInstance);
        
        var instance = method.Invoke(_dependenceFactory, null);

        return instance ?? throw new NullReferenceException($"Failed to create instance for type {typeInstance}");
    }
    
    public void FinalizeBinding(BindingInfo bindingInfo)
    {
        _finalizeBindingService.FinalizeBinding(bindingInfo, ref _singleInstances, ref _manyInstance);
    }

    private void UnregisterInstance(Type type)
    {
        _singleInstances.Remove(type);
    }

    public void UnsubscribeAll()
    {
        _singleInstances.Clear();
        _manyInstance.Clear();
    }

    public void ClearAll()
    {
        _singleInstances.Clear();
        _manyInstance.Clear();
    }

    public void UnsubscribeInstance(Type type)
    {
        _singleInstances.Remove(type);
    }
}