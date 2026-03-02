using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Linq;
using UnRechnung.Models;
using UnRechnung.ViewModels;

namespace UnRechnung.Views;

public partial class InvoiceView : UserControl
{
  private Point _ghostPosition = new(0, 0);
  private readonly Point _mouseOffset = new(0, 0);

  public InvoiceView()
  {
    InitializeComponent();
    InvoiceItemsListBox.AddHandler(DragDrop.DragOverEvent, OnDragOver);
    InvoiceItemsListBox.AddHandler(DragDrop.DropEvent, OnDrop);
  }

  private void OnDragOver(object? sender, DragEventArgs e)
  {
    var currentPosition = e.GetPosition(InvoiceItemsListBox);
    double offsetX = currentPosition.X - _ghostPosition.X;
    double offsetY = currentPosition.Y - _ghostPosition.Y;

    GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);

    e.DragEffects = DragDropEffects.Move;
    if (DataContext is not InvoiceViewModel vm) return;

    var data = e.Data.Get("item");
    if (data is not InvoiceItem draggedItem) return;

    if (e.Data.Contains("item"))
    {
      e.DragEffects = DragDropEffects.Move;
    }
    else
    {
      e.DragEffects = DragDropEffects.None;
    }
  }

  private void OnDrop(object? sender, DragEventArgs e)
  {
    if (DataContext is not InvoiceViewModel vm ||
        sender is not ListBox listBox ||
        e.Data.Get("item") is not InvoiceItem draggedItem)
    {
      return;
    }

    var point = e.GetPosition(listBox);
    ListBoxItem targetContainer = listBox.GetVisualDescendants()
      .OfType<ListBoxItem>()
      .FirstOrDefault(i => i.Bounds.Contains(point));

    if (targetContainer?.DataContext is InvoiceItem targetItem &&
        targetItem != draggedItem)
    {
      vm.SwapItems(draggedItem, targetItem);
    }

    e.Handled = true;
  }

  private async void InvoiceItem_PointerPressedAsync(object? sender, PointerPressedEventArgs e)
  {
    if (sender is not Border border) return;
    if (border.DataContext is not InvoiceItem draggedItem) return;

    var ghostPos = GhostItem.Bounds.Position;
    _ghostPosition = new Point(ghostPos.X + _mouseOffset.X, ghostPos.Y + _mouseOffset.Y);

    var mousePos = e.GetPosition(InvoiceItemsListBox);
    double offsetX = mousePos.X - ghostPos.X;
    double offsetY = mousePos.Y - ghostPos.Y + _mouseOffset.X;
    GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);

    if (DataContext is not InvoiceViewModel vm) return;
    vm.StartDrag(draggedItem);

    GhostItem.IsVisible = true;

    var dragData = new DataObject();
    dragData.Set("item", draggedItem);
    _ = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
    GhostItem.IsVisible = false;
  }

  private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    if (sender is null) return;

    ListBox listBox = (ListBox)sender;

    listBox.ScrollIntoView(listBox.SelectedItem);
    //listBox.SelectedItem = null;
  }
}

