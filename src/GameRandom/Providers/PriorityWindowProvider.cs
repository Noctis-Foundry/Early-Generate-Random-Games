using Avalonia.Controls;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scripts.WindowServices;
using GameRandom.Src;

namespace GameRandom.Providers;

public class PriorityWindowProvider(Window mainWindow) : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.BindSingleton(typeof(TaskWaiterWindow), new TaskWaiterWindow(mainWindow));
    }
}