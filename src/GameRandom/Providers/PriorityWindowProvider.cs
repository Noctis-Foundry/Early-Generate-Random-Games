using Avalonia.Controls;
using GameRandom.DISystem.Providers;
using GameRandom.Scripts.WindowServices;

namespace GameRandom.Providers;

public class PriorityWindowProvider(Window mainWindow) : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.BindSingleton(typeof(TaskWaiterWindow), new TaskWaiterWindow(mainWindow));
    }
}