using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace MaterialFlow.Converters;

/// <summary>
/// Конвертер значень, що перетворює локальний файловий шлях до зображення у об'єкт Bitmap для Avalonia.
/// </summary>
public class PathToBitmapConverter : IValueConverter
{
    /// <summary>
    /// Конвертує шлях до файлу зображення (рядок) в об'єкт Bitmap.
    /// Якщо файл не існує або пошкоджений, повертає null.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                using var stream = File.OpenRead(path);
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// Зворотне перетворення не підтримується.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
