using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.Views;

namespace GameRandom.ViewModels;

public class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose()
    {
        
    }
}