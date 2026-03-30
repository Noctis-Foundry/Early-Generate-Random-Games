using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminPanelSystem;

namespace GameRandom.AvaloniaConverters;

public class DictionaryToListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Dictionary<int, AdminPanelElementData> dictionary)
        {
            var list = dictionary.Values.ToList();
            return list;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}