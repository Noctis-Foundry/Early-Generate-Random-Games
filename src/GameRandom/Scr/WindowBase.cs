using System;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Src;

/// <summary>
/// Base class for application windows with ViewModel support and lifecycle management.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type that inherits from ViewModelBase.</typeparam>
public abstract class WindowBase<TViewModel> : Window, IDisposable where TViewModel : ViewModelBase, new()
{
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

        IsActive = true;
        IsClosing = false;
        
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
    /// Displays a message to the user.
    /// </summary>
    /// <param name="message">Message text to display.</param>
    protected void ShowMessage(string message)
    {
        if (Di.Container.GetInstance<ErrorService>() is not ErrorService messageWindow)
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
        if (Di.Container.GetInstance<EventBus>() is not EventBus eventBus)
            throw new NullReferenceException(nameof(EventBus));
        
        eventBus.Subscribe<TEvent>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(task);
        });
    }

    /// <summary>
    /// Handles window closing event and performs cleanup.
    /// </summary>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;

        IsClosing = true;
        IsActive = false;
        
        Dispose();
        
        base.OnClosing(e);
    }

    protected virtual void InitializeDiContainer()
    {
        
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