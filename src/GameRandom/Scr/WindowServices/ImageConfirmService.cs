using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using GameRandom.Scr.DI;
using GameRandom.Views;

namespace GameRandom.Src;

public class ImageConfirmService(Window owner) : AbstractWindowService<ConfirmImage>(owner)
{
    public override void ShowWindow(object? data = null)
    {
        if (Di.Container.GetInstance<ErrorService>() is ErrorService errorService)
        {
            errorService.ShowWindow(new ErrorStruct{ErrorMessage = "This window cannot be open without async thread"});
            return;
        }
        
        base.ShowWindow(data);
    }

    public override async Task<Bitmap?> ShowWindowAsync(object? data = null)
    {
        if (ControlWindow.IsActive)
        {
            return null;
        }
        
        ControlWindow = new ConfirmImage();
        var bitmap = await ControlWindow.ShowImageDialogWindow(OwnerWindow);
        
        return bitmap;
    }
}