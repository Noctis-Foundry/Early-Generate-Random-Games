using System;
using Avalonia.Threading;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminConfirmSystem.ContextWindowViewModels;

public class TaskProcessingViewModel : ViewModelBase
{
    private DispatcherTimer? _dispatcherTimer;
    private EventHandler? _timerCallback;
    private Func<bool>? _isProcessingReference;
    
    private const int TimerIntervalMs = 100;

    private Action? _isClosing;

    public void InitializeTimer(Func<bool> isProcessing)
    {
        _isProcessingReference = isProcessing;
        
        _timerCallback = (sender, args) =>
        {
            if (!_isProcessingReference())
            {
                _dispatcherTimer?.Stop();
                _dispatcherTimer?.Tick -= _timerCallback;
                _isClosing?.Invoke();
            }
        };
        
        StartTimer();
    }
    
    private void StartTimer()
    {
        if (_dispatcherTimer is not null)
            return;

        _dispatcherTimer = new DispatcherTimer();

        _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(TimerIntervalMs);
        _dispatcherTimer.Tick += _timerCallback;
        _dispatcherTimer.Start();
    }
    
    public void SetClosingCallback(Action closingHandler)
    {
        _isClosing += closingHandler;
    }
    
    public void UnsubscribeClosing(Action closingHandler)
    {
        _isClosing -= closingHandler;
    }

    public override void Dispose()
    {
        _dispatcherTimer?.Tick -= _timerCallback;
        _dispatcherTimer?.Stop();
        _dispatcherTimer = null;
        
        _timerCallback = null;
        _isProcessingReference = null;
        _isClosing = null;
        
        base.Dispose();
    }
}