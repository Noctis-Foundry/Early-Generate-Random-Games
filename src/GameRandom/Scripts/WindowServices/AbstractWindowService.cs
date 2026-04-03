using System.Collections;
using GameRandom.DependenceInjectSystem;
using System.Collections.Generic;
using GameRandom.DependenceInjectSystem;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;

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