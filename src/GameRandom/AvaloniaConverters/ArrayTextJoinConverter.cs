using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameRandom.AvaloniaConverters;

public class ArrayTextJoinConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HashSet<string> array)
        {
            string prefix = parameter as string ?? "N/A";
            string text = string.Join(", ", array);
            return $"{prefix}: {text}";
        }

        return "--";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}