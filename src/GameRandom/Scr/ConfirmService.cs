using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class ConfirmService
{
    private ConfirmDialog? _confirmDialog;
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    
    public async Task<bool> OpenConfirmDialog(string title, Window owner)
    {
        if (!await _semaphoreSlim.WaitAsync(0))
            return false;
        
        _confirmDialog = new ConfirmDialog();
        var result = await _confirmDialog.ShowConfirmDialog(title, owner);
        
        _semaphoreSlim.Release();
        
        return result;
    }
}