using Avalonia.Controls;
using Avalonia.Input;

namespace UnRechnung.Views;

public partial class ProductsView : UserControl
{
  public ProductsView()
  {
    InitializeComponent();
  }

  private void SearchProduct_TextBox_GotFocus(object? sender, GotFocusEventArgs e)
  {
    if (sender is TextBox tb)
    {
      tb.SelectAll();
    }
  }

  private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    if (sender is null) return;

    DataGrid table = (DataGrid)sender;

    table.ScrollIntoView(table.SelectedItem, table.CurrentColumn);
  }
}

