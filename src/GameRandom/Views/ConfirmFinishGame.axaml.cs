using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class ConfirmFinishGame : Window
{
    private ConfirmFinishGameViewModel? _viewModel;

    public ConfirmFinishGame()
    {
        InitializeComponent();
        _viewModel = new ConfirmFinishGameViewModel();
        DataContext = _viewModel;
    }

    private async void OnSaveEditClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel != null)
            await _viewModel.SaveEditAsync();
    }

    private async void OnChooseImageClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        var file = await _viewModel.ChooseImageAsync(StorageProvider);
        if (file != null)
        {
            await using var stream = await file.OpenReadAsync();
            _viewModel.ImageBitmap = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));
        }
    }
}