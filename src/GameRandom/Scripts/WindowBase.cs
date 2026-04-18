using System;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scripts.WindowServices;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.Src;

/// <summary>
/// Base class for application windows with ViewModel support and lifecycle management.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type that inherits from ViewModelBase.</typeparam>
public abstract class WindowBase<TViewModel> : Window, IDisposable where TViewModel : ViewModelBase, new()
{
    /// <summary>
    /// Task runner for launch methods with try/catch structure
    /// </summary>
    [Inject] protected TaskRunner TaskRunner = null!;
    
    /// <summary>
    /// Saved handler for processing operations.
    /// </summary>
    protected Action SavedProcessingHandler;
    
    /// <summary>
    /// Indicates whether the window is currently active.
    /// </summary>
    protected bool IsActive;
    
    /// <summary>
    /// Indicates whether the window is in the process of closing.
    /// </summary>
    protected bool IsClosing;
    
    /// <summary>
    /// Initializes the ViewModel and sets it as DataContext.
    /// </summary>
    protected virtual void InitializeViewModel()
    {
        var vm = new TViewModel();
        DataContext = vm;
    }

    /// <summary>
    /// Shows the window if it's not already active.
    /// </summary>
    public override void Show()
    {
        if (IsActive)
            return;

        SetActivity();
        
        base.Show();
    }
    
    /// <summary>
    /// Initializes the processing handler for long-running operations.
    /// </summary>
    /// <param name="hostWindow">Optional host window for the processing dialog.</param>
    protected void InitializeProcessingHandler(Window hostWindow = null!)
    {
        if (DataContext is not ViewModelBase vm)
            return;
        
        SavedProcessingHandler = () =>
        {
            if (Di.ResolveInstance.TryGetInstance<TaskWaiterWindow>() is not { } waiter)
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
    /// Displays a message to the user.
    /// </summary>
    /// <param name="message">Message text to display.</param>
    protected void ShowMessage(string message)
    {
        if (Di.ResolveInstance.TryGetInstance<ErrorService>() is not { } messageWindow)
            throw new NullReferenceException(nameof(ErrorService));
        
        messageWindow.ShowWindow(new ErrorStruct{ErrorMessage = message, ErrorType = ErrorEnum.Message});
    }

    /// <summary>
    /// Subscribes to an event from the EventBus and executes the task on UI thread.
    /// </summary>
    /// <typeparam name="TEvent">Event type to subscribe to.</typeparam>
    /// <param name="task">Action to execute when event is received.</param>
    protected void InitializeEventBusListener<TEvent>(Action task)
    {
        if (Di.ResolveInstance.TryGetInstance<EventBus>() is not { } eventBus)
            throw new NullReferenceException(nameof(EventBus));
        
        eventBus.Subscribe<TEvent>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(task);
        });
    }

    protected void SetInactive()
    {
        IsClosing = true;
        IsActive = false;
    }
    
    protected void SetActivity()
    {
        IsClosing = false;
        IsActive = true;
    }
    
    /// <summary>
    /// Handles window closing event and performs cleanup.
    /// </summary>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;

        SetInactive();
        
        Dispose();
        
        base.OnClosing(e);
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

    protected TViewModel GetViewModel()
    {
        if (DataContext is TViewModel vm)
            return vm;

        return null!;
    }
    
    /// <summary>
    /// Disposes window resources and resets state.
    /// </summary>
    public virtual void Dispose()
    {
        if (DataContext is ViewModelBase vm)
        {
            vm.StartProcessing -= SavedProcessingHandler;
            vm.Dispose();
        }

        DataContext = null;
        
        SavedProcessingHandler = null!;

        IsClosing = false;
        IsActive = false;
    }
}