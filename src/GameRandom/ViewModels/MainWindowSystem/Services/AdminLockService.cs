using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.MainWindowSystem.Interface;

namespace GameRandom.ViewModels.MainWindowSystem.Services;

public class AdminLockService : IDisposable, IAdminLock
{
    [Inject] private EventBus _eventBus = null!;

    private readonly BehaviorSubject<bool> _isAdmin = new(true);
    public IObservable<bool> IsAdmin => _isAdmin.AsObservable();
    
    private Action<AdminRulesUpdate> _adminListener = null!;

    public void Initialize()
    {
        Di.ResolveInstance.ResolveFiled(out _eventBus);
        
        if (_eventBus is null)
            throw new NullReferenceException();
        
        _adminListener = _ => AdminListener();
        _eventBus.Subscribe(_adminListener);
    }

    private void AdminListener()
    {
        _isAdmin.OnNext(User.GetInstance().IsAdmin());
    }

    public void Dispose()
    {
        _isAdmin?.Dispose();
        
        _eventBus.Unsubscribe(_adminListener);
        _eventBus = null!;

        _adminListener = null!;
    }
}