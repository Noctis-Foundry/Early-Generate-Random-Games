using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class AdminRegistrationWindow : WindowAbstract
{
    [Inject] private PostgresListener? _postgresListener;
    
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        DataContext = new AdminRegistrationViewModel();

        Di.Container.ResolveField(out _postgresListener);
        
        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));
        
        _postgresListener.Subscribe(TableEnum.AdminTable, structure =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await CheckAdminStatus(structure);
            });
        });
    }

    private async Task CheckAdminStatus(PayloadStructure payload)
    {
        if (payload.TableCode != (int)TableEnum.AdminTable)
            return;

        if (Di.Container.GetInstance<DatabaseService>() is not DatabaseService databaseService)
            throw new NullReferenceException(nameof(DatabaseService));

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        
        try
        {
            if (!await databaseService.CheckInAdminStatus(User.GetInstance().GetUserId(), cancellation.Token))
            {
                if (DataContext is AdminRegistrationViewModel vm)
                    vm.Dispose();
                
                Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminRegistrationViewModel vm)
            vm.Dispose();
        
        Close();
    }
}