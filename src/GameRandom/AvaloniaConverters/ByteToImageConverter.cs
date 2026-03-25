using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GameRandom.Scr.DI;
using GameRandom.Src;

namespace GameRandom.AvaloniaConverters;

public class ByteToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Bitmap empty = new Bitmap(AssetLoader.Open(new Uri("avares://GameRandom/Assets/steamAwatarWithNight.jpg")));
        
        if (value is byte[])
        {
            if (Di.Container.GetInstance<SteamService>() is not SteamService steamService)
                throw new NullReferenceException(nameof(SteamService));
            
            Bitmap? bitmap = steamService.GetImageSyncFromBytes(value as byte[]);
            
            if (bitmap == null)
                return empty;
            
            return bitmap;
        }
        
        return empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}