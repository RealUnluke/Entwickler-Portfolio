using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Interfaces;
using UnRechnung.Models;
using UnRechnung.Services;

namespace UnRechnung.ViewModels
{
  public partial class InvoiceViewModel : ViewModelBase, IAsyncInitializable
  {
    [ObservableProperty]
    private Cursor _currentCursor = Cursor.Default;
    [ObservableProperty]
    private bool _isUiEnabled = true;

    // ==============================================================================
    // CUSTOMER DATA
    // ==============================================================================
    [ObservableProperty]
    private Customer? _selectedCustomer;
    [ObservableProperty]
    private DateTime _selectedInvoiceDate = DateTime.Now;
    [ObservableProperty]
    private DateTime _selectedInvoiceDeliveryDate = DateTime.Now;
    [ObservableProperty]
    private string _invoiceNumber = string.Empty;
    [ObservableProperty]
    private bool _isNewCustomer = false;

    [ObservableProperty]
    private Invoice _currentInvoice = new Invoice();
    [ObservableProperty]
    private Product? _selectedProduct;
    [ObservableProperty]
    private InvoiceItem? _draggingInvoiceItem;

    public ObservableCollection<Customer> CustomerSelection { get; set; } = new ObservableCollection<Customer>();

    // ==============================================================================
    // INVOICE POSITIONS
    // ==============================================================================
    [ObservableProperty]
    private int _selectedTableItemIndex;
    [ObservableProperty]
    private int _positionMax = 1;
    [ObservableProperty]
    private decimal _subTotal = 0.00m;
    [ObservableProperty]
    private decimal _netTotal = 0.00m;
    [ObservableProperty]
    private decimal _salesTax = 0.00m;
    [ObservableProperty]
    private decimal _endTotal = 0.00m;

    [ObservableProperty]
    private WriteableBitmap? _pdfPreviewImage;

    public ObservableCollection<InvoiceItem> CurrentInvoiceItems { get; } = new ObservableCollection<InvoiceItem>();
    public ObservableCollection<Product> AllProducts { get; set; } = new ObservableCollection<Product>();


    private readonly AppDbContext _dbContext;
    private readonly IPdfService _pdfService;
    private readonly INotificationService _notificationService;

    /// <summary>Needed for the design view.</summary>
    public InvoiceViewModel() { }

    public InvoiceViewModel(AppDbContext dbContext, IPdfService pdfService, INotificationService notificationService)
    {
      _dbContext = dbContext;
      _pdfService = pdfService;
      _notificationService = notificationService;

      CurrentInvoiceItems.CollectionChanged += InvoiceItemsChanged;
      CurrentInvoice.Customer = SelectedCustomer;
      CurrentInvoice.InvoiceNumber = GetNextInvoiceNumberFromDb();

      CreateInvoiceItem();
    }

    public async Task InitializeAsync()
    {
      CustomerSelection = new ObservableCollection<Customer>(await _dbContext.Customers.Where(c => c.IsActive).AsNoTracking().ToListAsync());
      AllProducts = new ObservableCollection<Product>(await _dbContext.Products.ToListAsync());
    }

    // ==============================================================================
    // DRAG AND DROP
    // ==============================================================================
    #region DragAndDrop
    public void StartDrag(InvoiceItem item)
    {
      DraggingInvoiceItem = item;
    }

    public void SwapItems(InvoiceItem first, InvoiceItem second)
    {
      int indexA = CurrentInvoiceItems.IndexOf(first);
      int indexB = CurrentInvoiceItems.IndexOf(second);

      if (indexA == -1 || indexB == -1 || indexA == indexB)
      {
        return;
      }

      (CurrentInvoiceItems[indexA], CurrentInvoiceItems[indexB]) = (CurrentInvoiceItems[indexB], CurrentInvoiceItems[indexA]);

      for (int i = 0; i< CurrentInvoiceItems.Count; i++)
      {
        CurrentInvoiceItems[i].Position = i + 1;
      }
    }
    #endregion

    // ==============================================================================
    // CUSTOMER DATA
    // ==============================================================================
    #region CustomerData
    partial void OnSelectedCustomerChanged(Customer? value)
    {
      InvoiceNumber = GetNextInvoiceNumberFromDb();
    }

    private string GetNextInvoiceNumberFromDb()
    {
      string invoiceNumber = string.Empty;
      NumberRange invoiceNumberRange = _dbContext.NumberRanges.Where(n => n.Type == NumberRange.eType.Invoice).FirstOrDefault();

      // Build the new invoice number
      // prefix + format + padding
      if (invoiceNumberRange != null)
      {
        invoiceNumber = invoiceNumberRange.GetInvoiceNumberString();
      }

      return (invoiceNumber);
    }
    #endregion

    // ==============================================================================
    // INVOICE POSITIONS
    // ==============================================================================
    #region InvoicePositions
    [RelayCommand]
    private void AddItem()
    {
      CreateInvoiceItem();
      SelectedTableItemIndex = CurrentInvoiceItems.Count - 1;
    }

    [RelayCommand]
    private void RemoveItem(InvoiceItem itemToRemove)
    {
      CurrentInvoiceItems.Remove(itemToRemove);
      --PositionMax;
    }

    private void CreateInvoiceItem()
    {
      InvoiceItem newItem = new InvoiceItem();

      newItem.Product = new Product();
      newItem.Product.Name = string.Empty;
      newItem.Product.Description = string.Empty;
      newItem.Product.UnitPrice = 0.00m;
      newItem.Product.Taxes = 19.00m;
      newItem.Position = PositionMax;
      newItem.Invoice = CurrentInvoice;
      newItem.Name = string.Empty;
      newItem.Description = string.Empty;
      newItem.Unit = "Stk.";
      newItem.Quantity = 1;
      newItem.UnitPrice = 0.00m;
      newItem.Discount = 0.00m;
      newItem.Taxes = 19.00m;
      newItem.Total = newItem.Quantity * newItem.UnitPrice;

      CurrentInvoiceItems.Add(newItem);

      ++PositionMax;
    }

    private void InvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (InvoiceItem oldItem in e.OldItems)
        {
          oldItem.PropertyChanged -= InvoiceItem_PropertyChanged;
        }
      }

      if (e.NewItems != null)
      {
        foreach (InvoiceItem newItem in e.NewItems)
        {
          newItem.PropertyChanged += InvoiceItem_PropertyChanged;
        }
      }

      UpdateEndCalculation();
    }

    private void InvoiceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(InvoiceItem.Total))
      {
        UpdateEndCalculation();
      }
    }

    private void UpdateEndCalculation()
    {
      // Reset SubTotal
      SubTotal = 0.00m;

      foreach (var item in CurrentInvoiceItems)
      {
        SubTotal += item.Total;
      }

      // Subtract total discount from sub total
      NetTotal = SubTotal;
      SalesTax = NetTotal * 0.19m;
      EndTotal = NetTotal * 1.19m;
    }
    #endregion

    [RelayCommand]
    private void CompleteInvoice()
    {
      Invoice newInvoice = new Invoice();
      InvoiceItem newItem = new InvoiceItem();
      newInvoice.InvoiceNumber = InvoiceNumber;
      newInvoice.CustomerId = SelectedCustomer.CustomerNumber;
      newInvoice.InvoiceDate = SelectedInvoiceDate.Date;
      newInvoice.DueDate = SelectedInvoiceDeliveryDate.Date;
      newInvoice.Status = Invoice.eStatus.Open;
      newInvoice.TotalAmount = EndTotal;

      foreach (var item in CurrentInvoiceItems)
      {
        newItem.CopyFrom(item);
        newInvoice.Items.Add(item);
      }

      // Update number range for new invoice
      NumberRange numberRange = _dbContext.NumberRanges.Where(n => n.Type == NumberRange.eType.Invoice).FirstOrDefault() ?? new NumberRange();
      ++numberRange.NextNumber;

      _dbContext.Invoices.Add(newInvoice);
      _dbContext.SaveChanges();

      _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, "Rechnung erfolgreich erstellt.");

      // Create PDF file and preview
      EnableUi(false);

      var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
      var invoicePath = Path.Combine(desktopFolder, "Rechnungen");
      Directory.CreateDirectory(invoicePath);
      var pdfPath = Path.Combine(invoicePath, $"{newInvoice.InvoiceNumber}.pdf");
      if (!_pdfService.CreatePdf(pdfPath, InvoiceNumber))
      {
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Error, "Die Datei konnte nicht gespeichert werden.");
      }

      PdfPreviewImage = _pdfService.RenderPdfPageAsBitmap(pdfPath);
      EnableUi(true);
    }

    private void EnableUi(bool enable)
    {
      CurrentCursor = (enable) ? Cursor.Default : Cursor.Parse("Wait");
      IsUiEnabled = enable;
    }
  }
}
