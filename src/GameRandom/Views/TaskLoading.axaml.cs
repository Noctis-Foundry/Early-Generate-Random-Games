using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameRandom.SteamSDK;
using GameRandom.ViewModels.AdminSystem.ContextWindowViewModels;

namespace GameRandom.Views;

public partial class TaskLoading : WindowAbstract
{
    private Action? _savedCloseHandler;
    
    public TaskLoading()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public void LoadingWindow(Func<bool> isProcessing)
    {
        var vm = new TaskProcessingViewModel();
        DataContext = vm;

        _savedCloseHandler = CloseProcessingWindow;
        
        vm.SetClosingCallback(_savedCloseHandler);
        vm.InitializeTimer(isProcessing);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing) return;

        IsClosing = true;
        e.Cancel = true;
        Close(true);
    }

    private void CloseProcessingWindow()
    {
        if (DataContext is TaskProcessingViewModel vm)
        {
            if (_savedCloseHandler is not null)
                vm.UnsubscribeClosing(_savedCloseHandler);

            vm.Dispose();
        }

        WaitBar.IsIndeterminate = false;
        
        DataContext = null;
        _savedCloseHandler = null;

        Owner = null;
        
        Close(true);
    }
}