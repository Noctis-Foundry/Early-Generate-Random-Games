using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Scr.DI;

namespace GameRandom.Src;

public abstract class AbstractWindowService<TWindow>(Window ownerWindow) : DependenceBase
    where TWindow : Window, new()
{
    protected readonly Window OwnerWindow = ownerWindow;
    protected TWindow ControlWindow = new TWindow();

    public virtual void ShowWindow(object? data = null)
    {
        if (!ControlWindow.IsActive)
            ControlWindow.Show();
    }
    public virtual async Task ShowWindowAsync(object? data = null)
    {
        if (!ControlWindow.IsActive)
            await ControlWindow.ShowDialog(OwnerWindow);
    }
}