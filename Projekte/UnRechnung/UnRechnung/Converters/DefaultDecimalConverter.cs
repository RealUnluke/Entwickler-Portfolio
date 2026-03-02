using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace UnRechnung.Converters
{
  public class DefaultDecimalConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is decimal decimalValue)
      {
        return (decimalValue.ToString("N2", CultureInfo.CurrentCulture));
      }

      return (value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is string stringValue)
      {
        // Remove blank spaces
        string cleanedValue = stringValue.Trim();

        // If only one decimal separator remains, keep value
        if (string.IsNullOrWhiteSpace(cleanedValue) || cleanedValue == "." || cleanedValue == ",")
        {
          return (AvaloniaProperty.UnsetValue); // Keep the previous value
        }

        if (decimal.TryParse(cleanedValue, NumberStyles.Currency, CultureInfo.CurrentCulture, out var result))
        {
          return (result);
        }
      }

      return (AvaloniaProperty.UnsetValue); // Prevent resetting to 0
    }
  }
}
