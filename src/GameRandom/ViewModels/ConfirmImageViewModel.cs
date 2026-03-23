using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using GameRandom.Scr.DI;

namespace GameRandom.ViewModels.AdminSystem;

/// <summary>
/// ViewModel for handling image selection from file system or clipboard.
/// Used for uploading game completion screenshots.
/// </summary>
public class ConfirmImageViewModel : ViewModelBase
{
    private Bitmap? _imageBitmap;
    
    /// <summary>
    /// Gets or sets the selected image bitmap.
    /// </summary>
    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }

    /// <summary>
    /// Opens a file picker dialog to select an image from the file system.
    /// Supports PNG, JPG, JPEG, and BMP formats.
    /// </summary>
    /// <param name="storageProvider">The storage provider for file system access.</param>
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

    /// <summary>
    /// Attempts to paste an image from the system clipboard.
    /// </summary>
    /// <param name="clipboard">The clipboard instance to read from.</param>
    public async Task PasteFromClipboardAsync(IClipboard? clipboard)
    {
        if (clipboard == null) return;

        var bitmap = await clipboard.TryGetBitmapAsync();
        
        if (bitmap is null)
            return;

        ImageBitmap = bitmap;
    }
}