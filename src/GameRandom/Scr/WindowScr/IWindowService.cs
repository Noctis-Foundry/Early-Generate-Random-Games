using System.Threading.Tasks;
using Avalonia.Controls;

namespace GameRandom.Scr.WindowScr;

public interface IWindowService
{
    Task ShowDialogAsync<TWindow>() where TWindow : Window, new();
}