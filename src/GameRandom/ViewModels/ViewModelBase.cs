using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.Views;

namespace GameRandom.ViewModels.AdminSystem;

public class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose()
    {
        
    }
}