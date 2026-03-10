using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class ConfirmService(Window owner)
{
    private ConfirmDialog? _confirmDialog;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private Window _owner = owner;
    
    public async Task<bool> OpenConfirmDialog(string title)
    {
        if (!await _semaphore.WaitAsync(0))
            return false;
        
        _confirmDialog = new ConfirmDialog();
        var result = await _confirmDialog.ShowConfirmDialog(title, _owner);
        
        _semaphore.Release();
        
        return result;
    }
}