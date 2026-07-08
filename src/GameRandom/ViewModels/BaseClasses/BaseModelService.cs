using System;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Database;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;

namespace GameRandom.ViewModels.BaseClasses;

public class BaseModelService : IDisposable
{
    [Inject] protected ErrorService ErrorService = null!;
    [Inject] protected DatabaseService DatabaseService = null!;

    protected const int DefaultDatabaseTimeLimitSecond = 5;
    protected const int SemaphoreTimeoutSec = 1;

    protected BaseModelService()
    {
        InitializeDiContainer();
    }
    
    
    /// <summary>
    /// Virtual method for initialize DI container and check nullability
    /// In default initialize ErrorService and DatabaseService
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    private void InitializeDiContainer()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (!IsDataNotNull(ErrorService))
            throw new NullReferenceException(nameof(ErrorService));
        if (DatabaseService is null)
            throw new NullReferenceException(nameof(DatabaseService));
    }
    
    /// <summary>
    /// Virtual method for checking object on null and logging error if it's null
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected bool IsDataNotNull(object? data)
    {
        if (data is null)
        {
            Logger.Error($"Data with type is null");
            return false;
        }

        return true;
    }

    public virtual void Dispose()
    {
        ErrorService = null!;
        DatabaseService = null!;
    }
}