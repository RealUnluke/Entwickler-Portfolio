using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using UnRechnung.Models;

namespace UnRechnung.Converters
{
  public class EnumToStatusConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is Invoice.eStatus enumValue)
      {
        string statusString = string.Empty;

        switch (enumValue)
        {
          case Invoice.eStatus.Open: statusString = "Offen"; break;
          case Invoice.eStatus.Paid: statusString = "Bezahlt"; break;
          default:break;
        }

        return (statusString);
      }

      return (value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      return (AvaloniaProperty.UnsetValue); // Prevent resetting to 0
    }
  }
}
