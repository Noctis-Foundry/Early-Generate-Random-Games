using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DISystem;

namespace GameRandom.Scripts.WindowServices;

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