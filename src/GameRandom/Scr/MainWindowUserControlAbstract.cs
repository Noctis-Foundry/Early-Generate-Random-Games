using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Src;

public abstract class MainWindowUserControlAbstract : UserControl, IDisposable
{
    protected Action<string>? _changeWindowAction;
    protected Action SavedProcessingHandler;
    
    /// <summary>
    /// Registers navigation callback for content switching.
    /// </summary>
    public virtual void AddListener(Action<string> _onChangeContent) => _changeWindowAction = _onChangeContent;

    public abstract void Close(object? sender, RoutedEventArgs e);

    public virtual void Open()
    {
        
    }
    
    protected void InitializeProcessingHandler(Window hostWindow = null!)
    {
        if (DataContext is not ViewModelBase vm)
            return;
        
        SavedProcessingHandler = () =>
        {
            if (Di.Container.GetInstance<TaskWaiterWindow>() is not TaskWaiterWindow waiter)
                throw new NullReferenceException(nameof(TaskWaiterWindow));

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var isEnd = await waiter.ShowAsyncWaiter(vm.IsProcessing, hostWindow);

                if (isEnd)
                    Logger.Debug("Is exit from processing");
            });
        };

        vm.StartProcessing += SavedProcessingHandler;
    }
    
    /// <summary>
    /// Cleans up resources and nullifies references.
    /// </summary>
    public virtual void Dispose()
    {
        if (DataContext is ViewModelBase vm)
        {
            vm.StartProcessing -= SavedProcessingHandler;
            vm.Dispose();
        }

        DataContext = null;
        
        _changeWindowAction = null;
    }
}