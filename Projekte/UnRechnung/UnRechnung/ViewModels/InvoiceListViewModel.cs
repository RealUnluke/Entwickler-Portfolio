using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Interfaces;
using UnRechnung.Models;
using UnRechnung.Services;

namespace UnRechnung.ViewModels
{
  public partial class InvoiceListViewModel : ViewModelBase, IAsyncInitializable
  {
    [ObservableProperty]
    private int _selectedTableItemIndex = 0;
    [ObservableProperty]
    private string _invoiceSearchText = string.Empty;

    public ObservableCollection<Invoice>? TableItems { get; set; }

    private readonly AppDbContext _dbContext;
    private readonly INavigationService _navigationService;

    /// <summary>Needed for the design view.</summary>
    public InvoiceListViewModel() { }

    public InvoiceListViewModel(AppDbContext dbContext, INavigationService navigationService)
    {
      _dbContext = dbContext;
      _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
      TableItems = new ObservableCollection<Invoice>(await _dbContext.Invoices.ToListAsync());
      LoadCustomerNames();
    }

    [RelayCommand]
    public void AddInvoice()
    {
      _navigationService.NavigateTo<InvoiceViewModel>();
    }

    [RelayCommand]
    public void SaveTableData()
    {
      // _dbContext.SaveChanges();
    }

    [RelayCommand]
    private async Task ExportInvoicesAsync()
    {
      // TODO - Not implemented yet
    }

    partial void OnInvoiceSearchTextChanged(string value)
    {
      TableItems.Clear();

      // Search invoice number
      if (int.TryParse(value, out var invoiceNumber))
      {
        foreach (var invoice in _dbContext.Invoices.Where(i => i.InvoiceNumber.Contains(value)))
        {
          TableItems.Add(invoice);
        }
      }
      else
      {
        // Search invoice customer name
        foreach (var invoice in _dbContext.Invoices.Where(i => (i.Customer.Name.ToLower().Contains(value.ToLower())) || (i.Customer.Representative.ToLower().Contains(value.ToLower()))))
        {
          TableItems.Add(invoice);
        }
      }

      LoadCustomerNames();
    }

    private void LoadCustomerNames()
    {
      Customer? customer;

      foreach (var invoice in TableItems)
      {
        customer = _dbContext.Customers.Where(c => (c.CustomerNumber == invoice.CustomerId)).FirstOrDefault();

        if (customer != null)
        {
          invoice.Customer.Name = customer.Name;
          invoice.Customer.Representative = customer.Representative;
        }
      }
    }
  }
}
