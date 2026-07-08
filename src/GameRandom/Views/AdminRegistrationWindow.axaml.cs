using Avalonia.Controls;
using Avalonia.Interactivity;
using GameRandom.Scripts;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminRegistrationSystem;

namespace GameRandom.Views;

public sealed partial class AdminRegistrationWindow : WindowBase<AdminRegistrationViewModel>
{
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeProcessingHandler();
        InitializeEventBusListener<AdminRulesUpdate>(CheckAdminStatus);

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
    
    private void CheckAdminStatus()
    {
        if (!User.GetInstance().IsTopLevelAdmin())
        {
            Dispose();
            Close();
        }
    }

    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        Dispose();
        
        Close();
    }
}