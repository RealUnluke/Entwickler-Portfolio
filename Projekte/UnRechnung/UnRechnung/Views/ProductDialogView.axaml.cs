using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System.Globalization;

namespace UnRechnung.Views;

public partial class ProductDialogView : Window
{
  public ProductDialogView()
  {
    InitializeComponent();
  }

  private void SelectAll(object? sender, GotFocusEventArgs e)
  {
    if (sender is TextBox tb)
    {
      Dispatcher.UIThread.Post(() =>
      {
        if (!string.IsNullOrEmpty(tb.Text))
        {
          //tb.Text = tb.Text.Trim([' ', '€', '%']);
          tb.SelectAll();
        }
      });
    }
  }

  private void NumericOnly(object? sender, KeyEventArgs e)
  {
    // Comma is allowed including numpad
    // Tab is also allowed
    if (e.Key == Key.OemComma || e.Key == Key.Decimal || e.Key == Key.Tab) return;

    // Only number keys including numpad
    if ((e.Key < Key.D0 || e.Key > Key.D9) && (e.Key < Key.NumPad0 ||e.Key > Key.NumPad9))
    {
      e.Handled = true;
    }
  }

  private void FormatOnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
  {
    if (sender is TextBox tb)
    {
      if (string.IsNullOrEmpty(tb.Text))
      {
        tb.Text = "0,00";
      }
    }
  }
}