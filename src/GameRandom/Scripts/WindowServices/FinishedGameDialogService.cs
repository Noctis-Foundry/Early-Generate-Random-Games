using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DbContext;
using GameRandom.Views;
using Logger = GameRandom.Scripts.Service.Logger;

namespace GameRandom.Scripts.WindowServices;

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