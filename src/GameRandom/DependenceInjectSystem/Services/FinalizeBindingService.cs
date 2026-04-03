using System;
using System.Collections.Generic;
using GameRandom.DependenceInjectSystem.Binders;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Enums;

namespace DIContainer.DiSystem.Services;

public class FinalizeBindingService(Func<Type, object> createInstance)
{
    private Func<Type, object> _createInstance = createInstance;
    
     public void FinalizeBinding(BindingInfo bindingInfo, ref Dictionary<Type, DependenceInfo> singleInstance, ref Dictionary<Type, Func<object>> manyInstance)
    {
        var (contractType, implementationType) = NormalizeBinding(bindingInfo);
        
        var dependenceInfo = CreateDependenceInfo(bindingInfo, implementationType);
        
        if (!bindingInfo.IsLazy && bindingInfo.ScopeType != ScopeType.Many) //создаем instance в dependenceInfo
        {
            dependenceInfo.Instance = _createInstance.Invoke(dependenceInfo.ImplementationType);
        }

        if (bindingInfo.ScopeType == ScopeType.Singleton)
            singleInstance.TryAdd(contractType, dependenceInfo);
        else
            manyInstance.TryAdd(contractType, () => _createInstance.Invoke(dependenceInfo.ImplementationType));
    }

    private (Type Key, Type Value) NormalizeBinding(BindingInfo binding)
    {
        CheckInArgumentsError(binding);
        
        if (binding.ContractType == null)
            return (binding.ImplementationType, binding.ImplementationType);

        if (binding.ImplementationType == null)
            return (binding.ContractType, binding.ContractType);

        return (binding.ContractType, binding.ImplementationType);
    }

    /// <summary>
    /// Creating dependence info, if Contract and ImplementationType is null throw argument exception
    /// </summary>
    /// <param name="bindingInfo"></param>
    /// <param name="implementationType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private DependenceInfo CreateDependenceInfo(BindingInfo bindingInfo, Type implementationType)
    {
        if (bindingInfo.ContractType is null && bindingInfo.ImplementationType is null) //Выкидываем если оба типа = null
            throw new ArgumentException("Failed to finalize binding. Contract and Implementation type is null");

        return new DependenceInfo(implementationType, bindingInfo.ScopeType, bindingInfo.IsLazy);
    }

    private void CheckInArgumentsError(BindingInfo bindingInfo)
    {
        if (bindingInfo.ContractType is not null && bindingInfo.ImplementationType is null && bindingInfo.ContractType.IsInterface)
            throw new ArgumentException("Failed to finalize binding. Implementation type is null");
        
        if (bindingInfo.ContractType == null && bindingInfo.ImplementationType == null)
            throw new ArgumentException("Binding invalid");
    }
}