using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Src;

public abstract class MainWindowUserControlAbstract : UserControl, IDisposable
{
    [Inject] protected TaskRunner TaskRunner = null!;
    
    protected Action<string>? _changeWindowAction;
    protected Action SavedProcessingHandler;
    protected bool IsInitializeTaskWaiter = false;
    
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
            if (Di.ResolveInstance.TryGetInstance<TaskWaiterWindow>() is not TaskWaiterWindow waiter)
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
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        if (!IsInitializeTaskWaiter)
            return;
        
        if (TopLevel.GetTopLevel(this) is Window window)
            InitializeProcessingHandler(window);
        else
            Logger.Error("Failed to find top level window");
    }

    /// <summary>
    /// Method for initialize all dependencies via IoC container
    /// Is base initialize all dependence from class instance
    /// Check in null TaskRunner dependency
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    protected virtual void InitializeDiContainer()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (TaskRunner == null)
            throw new NullReferenceException("Failed to inject Task Runner");
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