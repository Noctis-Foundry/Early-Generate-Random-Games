using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GameRandom.CoreApp;

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

    public static void ConvertListToJson(List<AppSavedContext>? list, string path)
    {
        if (list == null || list.Count == 0)
        {
            Console.WriteLine("List is empty or null. Nothing to save.");
            return;
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };

            string json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(path, json);
            Console.WriteLine($"Saved {list.Count} items to {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving JSON: {ex.Message}");
        }
    }
}