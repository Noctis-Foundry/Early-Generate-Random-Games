using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameRandom.AvaloniaConverters;

public class BoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isFinished)
            return isFinished ? "Completed" : "In Progress";

        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Обычно ConvertBack не нужен, можно просто вернуть false
        return value is string str && str.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }
}