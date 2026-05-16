using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MaterialFlow.ViewModels;

public class MessageColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string msg && !string.IsNullOrEmpty(msg))
        {
            bool isSuccess = msg.Contains("successful", StringComparison.OrdinalIgnoreCase);
            bool isBg = parameter?.ToString() == "bg";

            if (isSuccess)
                return isBg ? Brush.Parse("#E8F5E9") : Brushes.Green;
            
            return isBg ? Brush.Parse("#FFEBEE") : Brushes.Red;
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
