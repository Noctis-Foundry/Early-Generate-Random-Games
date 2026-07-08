using System;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DbContext;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.WindowServices;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.ViewModels.AdminPanelSystem;

public class AdminPanelFactory
{
    public AdminPanelElementData? CreateAdminPanelElement(Users user, FinishedGames finishedGame)
    {
        RelayCommand openConfirmWindow = new RelayCommand(() =>
        {
            if (Di.ResolveInstance.TryGetInstance<AdminConfirmService>() is not AdminConfirmService confirmService)
                throw new NullReferenceException("Injecting is not successful");
            
            confirmService.ShowWindow(finishedGame);
        });
        
        if (string.IsNullOrEmpty(user.Nickname))
            return null;

        return new AdminPanelElementData(finishedGame, openConfirmWindow, user.Nickname);
    }
}