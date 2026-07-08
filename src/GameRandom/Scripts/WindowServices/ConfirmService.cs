using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Views;

namespace GameRandom.Scripts.WindowServices;

public class ConfirmService(Window owner)
{
    private ConfirmDialog _confirmDialog;

    private Window _owner = owner;
    
    public async Task<bool> OpenConfirmDialog(string title)
    {
        if (_confirmDialog.IsVisible)
            return false;
        
        var result = await _confirmDialog.ShowConfirmDialog(title, _owner);
        
        return result;
    }
}