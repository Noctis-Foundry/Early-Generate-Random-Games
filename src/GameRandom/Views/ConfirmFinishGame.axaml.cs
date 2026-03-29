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
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;
using Logger = GameRandom.Scr.Service.Logger;

namespace GameRandom.Views;

public sealed partial class ConfirmFinishGame : WindowBase<ConfirmFinishGameViewModel>
{
    private EventHandler<TextChangingEventArgs>? _textChanging;
    
    public ConfirmFinishGame()
    {
        InitializeComponent();
        InitializeViewModel();
        InitializeCommentListener();
        InitializeProcessingHandler();
        
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private void InitializeCommentListener()
    {
        _textChanging = (sender, args) =>
        {
            if (DataContext is ConfirmFinishGameViewModel vm && sender is TextBox textBox)
            {
                vm.Comment = textBox.Text;
            }
        };

        CommentBox.TextChanging += _textChanging;
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
        if (DataContext is ConfirmFinishGameViewModel vm)
        {
           var isEdit = await vm.SaveEditAsync();
           
           if (isEdit) Close();
        }
    }

    private async void ChooseImageFromFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var imageConfirmService = GetImageConfirm();

        var bitmap = await imageConfirmService.ConfirmFromFile(StorageProvider);
        
        if (bitmap is not null && GetViewModel() is { } vm)
        {
            vm.ImageBitmap = bitmap;
        }
    }
    private async void ChooseImageFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var imageConfirmService = GetImageConfirm();

        var bitmap = await imageConfirmService.PasteFromClipboardAsync(Clipboard);
        
        if (bitmap is not null && GetViewModel() is { } vm)
        {
            vm.ImageBitmap = bitmap;
        }
    }

    private ImageConfirmService GetImageConfirm()
    {
        if (Di.Container.GetInstance<ImageConfirmService>() is not ImageConfirmService imageConfirmService)
            throw new NullReferenceException(nameof(ImageConfirmService));

        return imageConfirmService;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing) return;
        
        IsClosing = true;
        IsActive = false;
        
        if (DataContext is ConfirmFinishGameViewModel vm)
        {
            Close(vm.IsUpdated);
            Dispose();
            return;
        }
        
        Dispose();
        Close(false);
    }

    public override void Dispose()
    {
        CommentBox.TextChanging -= _textChanging;
        _textChanging = null;
        
        base.Dispose();
    }
}