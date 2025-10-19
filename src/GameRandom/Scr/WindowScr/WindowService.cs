using System.Threading.Tasks;
using Avalonia.Controls;

namespace GameRandom.Scr.WindowScr;

public class WindowService : IWindowService
{
    private readonly Window _owner;

    public WindowService(Window owner)
    {
        _owner = owner;
    }

    public async Task ShowDialogAsync<TWindow>() where TWindow : Window, new()
    {
        var window = new TWindow();
        await window.ShowDialog(_owner);
    }
}