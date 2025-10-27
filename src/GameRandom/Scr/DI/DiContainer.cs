using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameRandom.Scr.Service;

namespace GameRandom.Scr.DI;

public class DiContainer
{
    private readonly Dictionary<Type, object> _instanceService = new Dictionary<Type, object>();
    private const string ClassName = "DiContainer";

    public DiContainer()
    {
        RegisterSingleInstance<DiContainer>(this);
    }
    
    public void RegisterSingleInstance<TInterface>(TInterface instance)
    {
        if (instance == null)
        {
            Console.WriteLine($"instance '{typeof(TInterface)}' is null");
            return;
        }
        
        _instanceService.Add(typeof(TInterface), instance);
    }
    public object GetInstance<TType>()
    {
        var type = typeof(TType);
        
        if (!_instanceService.ContainsKey(type))
            throw new Exception($"Not founded object with type {type}");

        var instance = _instanceService[type];
        
        return instance;
    }
    public object? TryGetInstance<TType>()
    {
        var type = typeof(TType);
        
        if (_instanceService.TryGetValue(type, out var instance))
        {
            return instance;
        }
        
        Console.WriteLine("Not found instance");
        return null;
    }

    public void InjectDependenciesAcrossAssembly()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var typesWithInjectFields = new List<Type>();

        foreach (var assembly in assemblies)
        {
            if (ShouldSkipAssembly(assembly))
                continue;
            
            try
            {
                var typesInAssembly = assembly.GetTypes().
                Where(t => t.IsClass && !t.IsAbstract).
                Where(t => HasInjectFields(t)).ToList();
                
                typesWithInjectFields.AddRange(typesInAssembly);
                
                if (typesInAssembly.Any())
                {
                    Console.WriteLine($"Found {typesInAssembly.Count} types in {assembly.GetName().Name}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"Could not fully load {assembly.GetName().Name}: {ex.Message}");
                
                var loadTypes = ex.Types.Where(t => t != null && t.IsClass && !t.IsAbstract).ToList();
                var injectableTypes = loadTypes.Where(t => t != null && HasInjectFields(t));
                
                typesWithInjectFields.AddRange(injectableTypes);
                continue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load {assembly.GetName().Name}: {ex.Message}");
                continue;
            }
        }
        
        Logger.Info($"Found {typesWithInjectFields.Count()} injection fields");
        
        foreach (var type in typesWithInjectFields)
        {
            var injectFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            Console.WriteLine($"Found item with type {type.FullName}");
            var instance = GetInstance(type);

            if (instance == null)
            {
                Logger.Error("Instance is null");
                continue;
            }
            
            InjectDependencies(instance, injectFields.ToList());
        }
    }

    private bool ShouldSkipAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? "";
    
        // Пропускаем системные сборки
        return name.StartsWith("System.") ||
               name.StartsWith("Microsoft.") ||
               name.StartsWith("Steamworks.") ||
               name.Contains("Steamworks.NET");
    }
    
    private object? GetInstance(Type type)
    {
        Console.WriteLine($"Getting instance of type {type.FullName}");
        return _instanceService.GetValueOrDefault(type);
    }
    
    private void InjectDependencies(object instance, List<FieldInfo> injectFields)
    {
        foreach (var field in injectFields)
        {
            if (_instanceService.TryGetValue(field.FieldType, out var value))
            {
                field.SetValue(instance, value);
                Logger.Info($"Injected {value.GetType().Name} into {instance.GetType().Name}.{field.Name}");
            }
            else
            {
                Logger.Error($"Cannot resolve dependency for field: {field.Name} of type {field.FieldType}");
            }
        }
    }

    private bool HasInjectFields(Type? type)
    {
        if (type == null)
            return false;
        
        return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Any(f => f.GetCustomAttribute<InjectAttribute>() != null);
    }
}