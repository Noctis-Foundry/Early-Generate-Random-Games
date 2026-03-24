using System;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.SteamSDK;

public abstract class WindowAbstract : Window
{
    public bool IsActive { get; private set; } = false;
    protected bool IsClosing = false;

    public virtual void Open(Window? parent = null)
    {
        if (IsActive) return;
        
        IsActive = true;
        IsClosing = false;

        if (parent is not null)
        {
            Show(parent);
            return;
        }
        
        Show();
    }
    
    public virtual void CloseWindow()
    {
        if (IsClosing) return;
        
        Hide();
        IsActive = false;
        IsClosing = false;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing) return;
        
        Hide();
        IsActive = false;
        e.Cancel = true;
        IsClosing = false;
    }
    
    protected void ProcessingWindowShow(ViewModelBase viewModelBase)
    {
        if (Di.Container.GetInstance<TaskWaiterWindow>() is not TaskWaiterWindow window)
            throw new NullReferenceException(nameof(window));
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var isCompleted = await window.ShowAsyncWaiter(viewModelBase.IsProcessing, this);
            
            if (isCompleted)
                Logger.Debug("Processing is end");
        });
    }
}