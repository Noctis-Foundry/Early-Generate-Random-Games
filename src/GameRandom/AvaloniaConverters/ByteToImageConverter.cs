using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GameRandom.SteamSDK;

namespace GameRandom.AvaloniaConverters;

public class ByteToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Bitmap empty = new Bitmap(AssetLoader.Open(new Uri("avares://GameRandom/Assets/steamAwatarWithNight.jpg")));
        
        if (value is byte[])
        {
            Bitmap? bitmap = SteamService.Instance.GetImageSyncFromBytes(value as byte[]);
            
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