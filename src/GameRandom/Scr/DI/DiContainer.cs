using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameRandom.Scr.Service;
using Microsoft.VisualBasic.FileIO;

namespace GameRandom.Scr.DI
{
    public class DiContainer
    {
        #region Private Fields

        private readonly Dictionary<Type, object> _instanceService = new Dictionary<Type, object>();

        #endregion
        
        #region DiRegister

        /// <summary>
        /// Registers an existing instance as a singleton for a given interface type.
        /// </summary>
        public void RegisterSingleInstance<TInterface>(TInterface instance)
        {
            if (instance == null)
            {
                Console.WriteLine($"instance '{typeof(TInterface)}' is null");
                return;
            }

            _instanceService[typeof(TInterface)] = instance;
        }

        public void Unregister<TInterface>()
        {
            _instanceService.Remove(typeof(TInterface));
        }

        #endregion

        /// <summary>
        /// Creates a new DI container and registers itself as a singleton instance.
        /// </summary>
        public DiContainer()
        {
            RegisterSingleInstance<DiContainer>(this);
        }

        #region Instance getters

        /// <summary>
        /// Returns the registered instance of the specified type. Throws an exception if not found.
        /// </summary>
        public object GetInstance<TType>()
        {
            var type = typeof(TType);

            if (!_instanceService.ContainsKey(type))
                throw new Exception($"Not founded object with type {type}");

            var instance = _instanceService[type];

            return instance;
        }

        /// <summary>
        /// Attempts to get the registered instance of the specified type. Returns null if not found.
        /// </summary>
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

        /// <summary>
        /// Internal method to get an instance by type. Returns null if not found.
        /// </summary>
        private object? GetInstance(Type type)
        {
            return _instanceService.GetValueOrDefault(type);
        }

        #endregion

        #region InjectionRegion

        /// <summary>
        /// Searches all assemblies for classes with fields marked with Inject attribute
        /// and injects registered dependencies into them.
        /// </summary>
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
                    var typesInAssembly = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract)
                        .Where(t => HasInjectFields(t))
                        .ToList();

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
                var injectFields = GetInjectFields(type);

                Console.WriteLine($"Found item with type {type.FullName}");
                var instance = GetInstance(type);

                if (instance == null)
                {
                    Logger.Error("Instance is null");
                    continue;
                }

                SetFieldInstanceValue(instance, injectFields.ToList());
            }
        }

        /// <summary>
        /// Determines whether a given assembly should be skipped (system assemblies and Steamworks are skipped).
        /// </summary>
        private bool ShouldSkipAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name ?? "";

            return name.StartsWith("System.") ||
                   name.StartsWith("Microsoft.") ||
                   name.StartsWith("Steamworks.") ||
                   name.Contains("Steamworks.NET");
        }

        /// <summary>
        /// Injects dependencies into fields of the given instance marked with Inject attribute.
        /// </summary>
        public void ResolveFieldsFromClassInstance(object? instance)
        {
            if (instance == null)
            {
                Logger.Error("DiContainer 'Inject Dependencies': instance is null");
                return;
            }
            
            SetFieldInstanceValue(instance, GetInjectFields(instance.GetType()));
        }

        /// <summary>
        /// Injects dependencies and returns the object if it is registered as object type; otherwise, instance is null.
        /// </summary>
        public bool ResolveField<TField>(out TField? instance) where TField : class
        {
            if (_instanceService.TryGetValue(typeof(TField), out var value))
            {
                instance = (TField)value;
                return true;
            }

            Logger.Error("DiContainer 'Inject Dependencies': value is not found");
            instance = null;
            return false;
        }

        /// <summary>
        /// Checks whether the type contains any fields marked with Inject attribute.
        /// </summary>
        private bool HasInjectFields(Type? type)
        {
            if (type == null)
                return false;

            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(f => f.GetCustomAttribute<InjectAttribute>() != null);
        }

        /// <summary>
        /// Find all injecting fields
        /// </summary>
        private List<FieldInfo> GetInjectFields(Type? type)
        {
            if (type == null)
            {
                Logger.Error("DiContainer 'Get Inject Fields': type is null");
                return new List<FieldInfo>();
            }

            var field = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null && !f.FieldType.IsValueType).ToList();

            return field;
        }

        /// <summary>
        /// Set value on instance object
        /// </summary>
        private void SetFieldInstanceValue(object instance, List<FieldInfo> injectFields)
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
                    throw new InvalidOperationException($"Dependency for {field.FieldType} not found");
                }
            }
        }
        
        #endregion
    }
}
