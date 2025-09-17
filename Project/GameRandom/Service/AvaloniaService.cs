using System;
using System.Text.Json;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GameRandom.Service;

public static class AvaloniaService
{
    public static Bitmap CreateBitmap(byte[] rawRgba, int width, int height)
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
    public static (int, string) ConvertApp(JsonElement app)
    {
        int appId = app.GetProperty("appid").GetInt32();
        string appName = app.GetProperty("name").GetString() ?? "Unknown";

        if (appId == 0 || appName == "Unknown")
        {
            Console.WriteLine("Dont find app");
            return (0, "Unknown");
        }

        return (appId, appName);
    }
}