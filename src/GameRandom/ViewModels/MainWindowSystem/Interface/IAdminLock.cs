using System;

namespace GameRandom.ViewModels.MainWindowSystem.Interface;

public interface IAdminLock
{
    public IObservable<bool> IsAdmin { get; }
    public void Initialize();
    public void Dispose();
}