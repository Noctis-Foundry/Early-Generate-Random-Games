using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using GameRandom.DataBaseContexts;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class FinishedGameDialogService (Window owner) : AbstractWindowService<ConfirmFinishGame>(owner)
{
    public override void ShowWindow(object? data = null)
    {
        return;
    }
    
    public override async Task<bool> ShowWindowAsync(object? data = null)
    {
        if (ControlWindow.IsActive)
            return false;

        if (data is not GameProgresses gameProgress)
        {
            throw new TypeLoadException("data must be GameProgresses type");
        }
        
        ControlWindow = new ConfirmFinishGame();
        return await ControlWindow.ShowAsync(OwnerWindow, gameProgress);
    }
}