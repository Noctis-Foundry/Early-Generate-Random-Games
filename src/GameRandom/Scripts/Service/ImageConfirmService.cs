using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using Avalonia.Input.Platform;
using GameRandom.DependenceInjectSystem;
using Avalonia.Media.Imaging;
using GameRandom.DependenceInjectSystem;
using Avalonia.Platform.Storage;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;

namespace GameRandom.Scr.Service;

public class ImageConfirmService : DependenceBase, IImageConfirm
{
    /// <summary>
    /// Opens a file picker dialog to select an image from the file system.
    /// Supports PNG, JPG, JPEG, and BMP formats.
    /// </summary>
    /// <param name="storageProvider">The storage provider for file system access.</param>
    public async Task<Bitmap?> ConfirmFromFile(IStorageProvider storageProvider)
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
            return new Bitmap(stream);
        }

        return null;
    }

    /// <summary>
    /// Attempts to paste an image from the system clipboard.
    /// </summary>
    /// <param name="clipboard">The clipboard instance to read from.</param>
    public async Task<Bitmap?> PasteFromClipboardAsync(IClipboard? clipboard)
    {
        if (clipboard == null) return null;

        var bitmap = await clipboard.TryGetBitmapAsync();
        
        return bitmap;
    }
}

public interface IImageConfirm
{
    public Task<Bitmap?> ConfirmFromFile(IStorageProvider storageProvider);
    public Task<Bitmap?> PasteFromClipboardAsync(IClipboard? clipboard);
}