using Avalonia.Controls;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Src;

namespace GameRandom.Providers;

public class AppInitializeProvider(Window mainWindow) : DiProvider
{
    private Window _ownerWindow = mainWindow;

    public override void BindingInstance()
    {
        DiContainer.BindInstance<IErrorService>().ToInstance(new ErrorService(_ownerWindow));
        
        DiContainer.BindSingleton(typeof(ErrorService), new ErrorService(_ownerWindow));
        DiContainer.BindSingleton(typeof(ConfirmService), new ConfirmService(_ownerWindow));
        DiContainer.BindSingleton(typeof(AdminConfirmService), new AdminConfirmService(_ownerWindow));
        DiContainer.BindSingleton(typeof(FinishedGameDialogService), new FinishedGameDialogService(_ownerWindow));
        DiContainer.BindSingleton(typeof(TaskWaiterWindow), new TaskWaiterWindow(_ownerWindow));
    }
}