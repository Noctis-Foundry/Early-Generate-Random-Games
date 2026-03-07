using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameRandom.AvaloniaConverters;

public class LongToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long id)
        {
            var prefix = parameter as string ?? string.Empty;
            return $"{prefix}: {id}";
        }
        
        return "Empty lobby ID";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}