using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using GameRandom.DataBaseContexts;
using GameRandom.Views;
using Logger = GameRandom.Scr.Service.Logger;

namespace GameRandom.Src;

public class FinishedGameDialogService (Window owner) : AbstractWindowService<ConfirmFinishGame>(owner)
{
    public override void ShowWindow(object? data = null)
    {
        throw new NotImplementedException();
    }
    
    public override async Task<bool> ShowWindowAsync(object? data = null)
    {
        Logger.Debug($"Control window is active status {ControlWindow.IsActive}");
        
        if (data is not GameProgresses gameProgress)
        {
            throw new TypeLoadException("data must be GameProgresses type");
        }
        
        ControlWindow = new ConfirmFinishGame();
        return await ControlWindow.ShowAsync(OwnerWindow, gameProgress);
    }
}