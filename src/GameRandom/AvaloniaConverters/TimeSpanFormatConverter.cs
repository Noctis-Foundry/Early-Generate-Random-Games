using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameRandom.AvaloniaConverters;

public class TimeSpanFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            string prefix = parameter as string ?? "N/A";
            return $"{prefix}: {dateTime:D}";
        }
        
        return "--";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}