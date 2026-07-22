using System;
using System.IO;
using Avalonia;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using Steamworks;

namespace GameRandom.Scripts.Service;

public class AvaloniaService
{
    private static Lazy<AvaloniaService> _lazyAvalonia = new (() => new AvaloniaService());
    
    public static AvaloniaService Instance => _lazyAvalonia.Value;
    
    private AvaloniaService(){}
    
    public Bitmap CreateBitmap(byte[] rawRgba, int width, int height)
    {
        byte[] bgra = new byte[rawRgba.Length];
        for (int i = 0; i < rawRgba.Length; i += 4)
        {
            bgra[i + 0] = rawRgba[i + 2]; // B
            bgra[i + 1] = rawRgba[i + 1]; // G
            bgra[i + 2] = rawRgba[i + 0]; // R
            bgra[i + 3] = rawRgba[i + 3]; // A
        }
        
        var bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96), // DPI
            PixelFormat.Bgra8888
        );
        
        using (var fb = bitmap.Lock())
        {
            System.Runtime.InteropServices.Marshal.Copy(bgra, 0, fb.Address, bgra.Length);
        }

        return bitmap;
    }

    public Bitmap? CreateSteamImage(int image)
    {
        if (image == 0)
            return null;
        
        uint width, height;
        SteamUtils.GetImageSize(image, out width, out height);

        byte[] imageByte = new byte[width * height * 4];
        SteamUtils.GetImageRGBA(image, imageByte, (int)(width * height * 4));

        var bitmap = CreateBitmap(imageByte, (int)width, (int)height);
        return bitmap;
    }
    
    /// <summary>
    /// Создает Bitmap из файла изображения по указанному пути в ресурсах приложения.
    /// </summary>
    /// <param name="path">Относительный путь к изображению в ресурсах. Пример: Assets/img.png</param>
    /// <returns>Bitmap изображения или null в случае ошибки.</returns>
    public Bitmap? CreateBitmapFromPath(string path)
    {
        var uri = new Uri($"avares://GameRandom/{path}");
        return new Bitmap(AssetLoader.Open(uri));
    }

    public GifImage CreateGifImageFromPath(string path)
    {
        var uri = new Uri($"avares://GameRandom/{path}");
        var gif = new GifImage
        {
            Source = GifStreamSource.FromUri(uri),
            Width = 120,
            Height = 70
        };

        return gif;
    }

    public byte[] ConvertToWebpBytes(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        ms.Position = 0;
        
        var originalBytes = ms.ToArray().Length * 8;
        
        Image<Rgba32> image = Image.Load<Rgba32>(ms);
        
        using var msWebp = new MemoryStream();
        var encoder = new WebpEncoder { Quality = 80, FileFormat = WebpFileFormatType.Lossy};
        image.Save(msWebp, encoder);

        var compressedBytes = msWebp.ToArray().Length * 8;
        
        Logger.Debug($"Original size = {originalBytes} and compressed bytes = {compressedBytes}");
        
        return msWebp.ToArray();
    }
    
    public Bitmap CreateBitmapFromBytes(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return new Bitmap(ms);
    }

    internal static void Reset()
    {
        _lazyAvalonia = new(() => new AvaloniaService());
    }

    public Bitmap? DefaultUserImage()
    {
        return CreateBitmapFromPath("Assets/steamAwatarWithNight.jpg");
    }
}