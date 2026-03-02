using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;

namespace YouTube_Downloader.Converters
{
  public class ETAConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      string convertedValue = string.Empty;

      if (value is int intValue)
      {
        if (intValue <= 0)
        {
          convertedValue = "Calculating...";
        }
        else
        {
          TimeSpan timeSpan = TimeSpan.FromSeconds(intValue);

          if (timeSpan.Minutes > 0)
          {
            convertedValue = $"{timeSpan.Minutes} minute";

            if (timeSpan.Minutes > 1)
            {
              convertedValue += "s";
            }
          }
          else if (timeSpan.Seconds > 0)
          {
            convertedValue = $"{timeSpan.Seconds} second";

            if (timeSpan.Seconds > 1)
            {
              convertedValue += "s";
            }
          }
        }
      }

      return (convertedValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      return (AvaloniaProperty.UnsetValue); // Prevent resetting to 0
    }
  }
}
