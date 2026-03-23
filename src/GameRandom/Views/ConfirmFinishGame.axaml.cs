using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK;
using GameRandom.ViewModels.AdminSystem;
using Logger = GameRandom.Scr.Service.Logger;

namespace GameRandom.Views;

public partial class ConfirmFinishGame : WindowAbstract
{
    private ConfirmFinishGameViewModel? _viewModel;

    public ConfirmFinishGame()
    {
        InitializeComponent();
        _viewModel = new ConfirmFinishGameViewModel();
        DataContext = _viewModel;

        CommentBox.TextChanging += (sender, args) =>
        {
            if (DataContext is ConfirmFinishGameViewModel vm && sender is TextBox textBox)
            {
                vm.Comment = textBox.Text;
            }
        };
        
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public async Task<bool> ShowAsync(Window owner, GameProgresses gameInfo)
    {
        IsClosing = false;
        if (DataContext is ConfirmFinishGameViewModel vm)
            vm.LoadData(gameInfo);
        return await ShowDialog<bool>(owner);
    }
    
    private async void OnSaveEditClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
           var isEdit = await _viewModel.SaveEditAsync();
           
           if (isEdit) Close();
        }
    }

    private async void OnChooseImageClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Di.Container.GetInstance<ImageConfirmService>() is ImageConfirmService imageConfirmService)
        {
            Bitmap? bitmap = await imageConfirmService.ShowWindowAsync();

            if (bitmap != null && DataContext is ConfirmFinishGameViewModel viewModel)
            {
                viewModel.ImageBitmap = bitmap;
            }
            else
                Logger.Error("bitmap from image confirm is null");
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing) return;
        
        e.Cancel = true;
        IsClosing = true;

        if (DataContext is ConfirmFinishGameViewModel vm)
        {
            Close(vm.IsUpdated);
            return;
        }

        Close(false);
    }
}