using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using GameRandom.SteamSDK;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class ConfirmImage : WindowAbstract
{
    public ConfirmImage()
    {
        InitializeComponent();
        DataContext = new ConfirmImageViewModel();

        Closing += (sender, args) =>
        {
            OnClosing(args);
        };
        
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public async Task<Bitmap?> ShowImageDialogWindow(Window owner)
    {
        return await ShowDialog<Bitmap>(owner);
    }
    
    private async void FileChoose(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is ConfirmImageViewModel vm)
            {
                await vm.ChooseFromFileAsync(StorageProvider);
            }
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    private async void ClipboardChoose(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is ConfirmImageViewModel vm)
            {
                await vm.PasteFromClipboardAsync(Clipboard);
            }
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing) return;
        
        IsClosing = true;
        e.Cancel = true;

        if (DataContext is ConfirmImageViewModel vm)
        {
            Close(vm.ImageBitmap);
            return;
        }
            
        Close();
    }
}