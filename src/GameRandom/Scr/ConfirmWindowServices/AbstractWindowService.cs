using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace GameRandom.SteamSDK;

public abstract class AbstractWindowService <TWindow> 
    where TWindow : WindowAbstract, new()
{
    protected readonly Window OwnerWindow;
    protected TWindow ControlWindow = new TWindow();

    public AbstractWindowService(Window ownerWindow)
    {
        OwnerWindow = ownerWindow;
        
        ControlWindow.Closing += async (sender, e) =>
        {
            ClosingWindow();
        };
    }

    public virtual void ShowWindow(object? data = null)
    {
        if (!ControlWindow.IsActive)
            ControlWindow.Open();
    }
    public virtual async Task ShowWindowAsync(object? data = null)
    {
        if (!ControlWindow.IsActive)
            await ControlWindow.ShowDialog(OwnerWindow);
    }
    protected virtual void ClosingWindow()
    {
        
    }
}