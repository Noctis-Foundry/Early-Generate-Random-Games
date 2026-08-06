using Avalonia.Controls;
using GameRandom.DISystem.Providers;
using GameRandom.Scripts.WindowServices;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;

namespace GameRandom.Providers;

public class WindowProvider(Window mainWindow) : DiProvider
{
    public override void BindingInstance()
    {
        var errorService = new ErrorService(mainWindow);
        
        DiContainer.BindInstance<IErrorService>().ToInstance(errorService);
        DiContainer.BindSingleton(typeof(ErrorService), errorService);
       
        DiContainer.BindSingleton(typeof(TaskWaiterWindow), new TaskWaiterWindow(mainWindow));
        DiContainer.BindSingleton(typeof(ConfirmService), new ConfirmService(mainWindow));
        DiContainer.BindSingleton(typeof(AdminConfirmService), new AdminConfirmService(mainWindow));
        DiContainer.BindSingleton(typeof(FinishedGameDialogService), new FinishedGameDialogService(mainWindow));
    }
}