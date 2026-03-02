using Avalonia;
using Avalonia.Data.Converters;
using System.Globalization;
using System;

namespace YouTube_Downloader.Converters
{
  public class SpeedConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      string convertedValue = string.Empty;

      if (value is decimal decValue)
      {
        if (decValue > 0)
        {
          if (decValue >= 1_000.0m)
          {
            if (decValue >= 1_000_000.0m)
            {
              convertedValue = $"{(decValue / 1_000_000.0m).ToString("0.00")} GB/s";
            }
            else
            {
              convertedValue = $"{(decValue / 1_000.0m).ToString("0.00")} MB/s";
            }
          }
          else
          {
            convertedValue = $"{decValue.ToString("0.00")} KB/s";
          }
        }
      }

      return (convertedValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is string stringValue)
      {
      }

      return (AvaloniaProperty.UnsetValue); // Prevent resetting to 0
    }
  }
}
