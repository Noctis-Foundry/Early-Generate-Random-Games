# Dependency Injection System

## Overview
Custom lightweight Dependency Injection (DI) system for managing service lifetimes and automatic dependency resolution. Supports singleton registration, field injection via attributes, and flexible initialization patterns.

## Architecture

### Di (Static Entry Point)
Static accessor providing global access to the DI container.

**Property**:
- `Container` - Returns singleton DiContainer instance

**Usage**:
```csharp
var container = Di.Container;
```

### DiContainer
Core container managing service registration, resolution, and automatic injection.

#### Registration

**RegisterSingleInstance<TInterface>(TInterface instance)**
Registers an existing instance as a singleton.

```csharp
container.RegisterSingleInstance<IService>(serviceInstance);
```

#### Resolution

**GetInstance<TType>()**
Returns registered instance. Throws exception if not found.

```csharp
var service = (IService)container.GetInstance<IService>();
```

**TryGetInstance<TType>()**
Returns registered instance or null if not found. Safe alternative to GetInstance.

```csharp
var service = container.TryGetInstance<IService>();
```

#### Automatic Injection

**InjectDependenciesAcrossAssembly()**
Scans all loaded assemblies for classes with [Inject] fields and automatically injects registered dependencies.

**Process**:
1. Scans all assemblies (skips System, Microsoft, Steamworks)
2. Finds classes with [Inject] attributed fields
3. Resolves dependencies from container
4. Sets field values via reflection

**Skipped Assemblies**:
- System.*
- Microsoft.*
- Steamworks.*

**ResolveFieldsFromClassInstance(object instance)**
Injects dependencies into specific instance fields marked with [Inject].

```csharp
container.ResolveFieldsFromClassInstance(myService);
```

**ResolveField<TField>(out TField instance)**
Resolves single dependency with out parameter pattern.

```csharp
if (container.ResolveField<IService>(out var service))
{
    // Use service
}
```

#### Internal Methods

- `HasInjectFields(Type type)` - Checks if type has [Inject] fields
- `GetInjectFields(Type type)` - Returns list of fields with [Inject] attribute
- `SetFieldInstanceValue(object instance, List<FieldInfo> fields)` - Sets field values from container
- `ShouldSkipAssembly(Assembly assembly)` - Filters system assemblies

### DiFactory
Factory for creating and registering instances with initialization support.

#### Methods

**Create<TClass, T1>(TClass instance, T1 arg1)**
Creates and registers class instance with single initialization argument.

```csharp
factory.Create<MyService, string>(new MyService(), "config");
```

**Create<TInterface, TClass, T1>(TClass instance, T1 arg1)**
Creates and registers instance as interface with single argument.

```csharp
factory.Create<IService, MyService, string>(new MyService(), "config");
```

**Create<TClass, T1, T2>(TClass instance, T1 arg1, T2 arg2)**
Creates and registers class instance with two initialization arguments.

**Create<TInterface, TClass, T1, T2>(TClass instance, T1 arg1, T2 arg2)**
Creates and registers instance as interface with two arguments.

**CreateDynamic<TClass>(TClass instance, params object[] args)**
Creates and registers instance using reflection for dynamic argument count.

```csharp
factory.CreateDynamic<MyService>(new MyService(), arg1, arg2, arg3);
```

**CreateDynamic<TInterface, TClass>(TClass instance, params object[] args)**
Creates and registers instance as interface using reflection.

### InjectAttribute
Marks fields for automatic dependency injection.

**Usage**:
```csharp
public class MyService
{
    [Inject] private readonly IDatabase _database = null!;
    [Inject] private readonly ILogger _logger = null!;
}
```

**Constraints**:
- Only works on non-public instance fields
- Field type must be reference type (not value type)

### Register (Abstract Base Class)
Base class for services requiring initialization before use.

**Methods**:
- `Init<T1>(T1 arg1)` - Override for single-argument initialization
- `Init<T1, T2>(T1 arg1, T2 arg2)` - Override for two-argument initialization
- `Init<TClass>(TClass instance, params object[] args)` - Override for dynamic initialization

**Usage**:
```csharp
public class MyService : Register
{
    public override void Init<T1>(T1 config)
    {
        // Initialize with config
    }
}
```

## Workflow

### 1. Registration Phase
```csharp
var factory = new DiFactory();
factory.Create<IDatabase, DatabaseService, string>(new DatabaseService(), connectionString);
factory.Create<ILogger, Logger>(new Logger());
```

### 2. Injection Phase
```csharp
Di.Container.InjectDependenciesAcrossAssembly();
```

### 3. Usage Phase
```csharp
public class GameService
{
    [Inject] private readonly IDatabase _database = null!;
    [Inject] private readonly ILogger _logger = null!;
    
    public void DoWork()
    {
        _database.Query();
        _logger.Log("Work done");
    }
}
```

## Features

- **Singleton Pattern**: All registered instances are singletons
- **Field Injection**: Automatic dependency injection via [Inject] attribute
- **Interface Registration**: Register implementations as interfaces
- **Flexible Initialization**: Support for 0-N initialization arguments
- **Assembly Scanning**: Automatic discovery of injectable fields
- **Type Safety**: Generic methods ensure compile-time type checking
- **Error Handling**: Throws exceptions for missing dependencies

## Limitations

- Only supports singleton lifetime (no transient or scoped)
- Field injection only (no constructor or property injection)
- Requires manual registration (no auto-registration by convention)
- Reflection-based injection has performance overhead

## Example: Complete Setup

```csharp
// 1. Create services
var database = new DatabaseService();
var logger = new LoggerService();
var eventBus = new EventBus();

// 2. Register with factory
var factory = new DiFactory();
factory.Create<IDatabaseService, DatabaseService, string>(database, "connection");
factory.Create<ILogger, LoggerService>(logger);
factory.Create<EventBus>(eventBus);

// 3. Inject dependencies
Di.Container.InjectDependenciesAcrossAssembly();

// 4. Services now have dependencies injected
// LobbyService, GameService, etc. automatically receive dependencies
```
