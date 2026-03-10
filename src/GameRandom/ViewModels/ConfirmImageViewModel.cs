using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace GameRandom.ViewModels;

public class ConfirmImageViewModel : ViewModelBase
{
    private Bitmap? _imageBitmap;
    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }

    public async Task ChooseFromFileAsync(IStorageProvider storageProvider)
    {
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                }
            }
        });

        if (files.Count > 0)
        {
            await using var stream = await files[0].OpenReadAsync();
            ImageBitmap = new Bitmap(stream);
        }
    }

    public async Task PasteFromClipboardAsync(IClipboard? clipboard)
    {
        if (clipboard == null) return;

        var bitmap = await clipboard.TryGetBitmapAsync();
        
        if (bitmap is null)
            return;

        ImageBitmap = bitmap;
    }
}