using System;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Service;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels;

public class LoadViewModel : ViewModelBase
{
    private AppLoading _appLoading = null!;

    private float _progress;

    public float Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private string _message;
    
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
    
    public LoadViewModel()
    {
        Di.ResolveInstance.ResolveFiled(out _appLoading);

        if (_appLoading == null)
            throw new NullReferenceException(nameof(_appLoading));
        
        _appLoading.OnChangeValue += UpdateValue;
    }

    private void UpdateValue()
    {
        Progress = _appLoading.Progress;
        Message = _appLoading.Message;
    }
}