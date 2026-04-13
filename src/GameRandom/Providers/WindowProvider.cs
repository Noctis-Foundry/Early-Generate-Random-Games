using Avalonia.Controls;
using GameRandom.DependenceInjectSystem.DiInterfaces;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Src;

namespace GameRandom.Providers;

public class WindowProvider(Window mainWindow) : DiProvider
{
    public override void BindingInstance()
    {
        DiContainer.BindInstance<IErrorService>().ToInstance(new ErrorService(mainWindow));
        DiContainer.BindSingleton(typeof(TaskWaiterWindow), new TaskWaiterWindow(mainWindow));
        
        DiContainer.BindSingleton(typeof(ConfirmService), new ConfirmService(mainWindow));
        DiContainer.BindSingleton(typeof(AdminConfirmService), new AdminConfirmService(mainWindow));
        DiContainer.BindSingleton(typeof(FinishedGameDialogService), new FinishedGameDialogService(mainWindow));
    }
}