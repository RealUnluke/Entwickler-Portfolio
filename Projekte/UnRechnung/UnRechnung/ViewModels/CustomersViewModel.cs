using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Interfaces;
using UnRechnung.Models;
using UnRechnung.Services;
using UnRechnung.Views;

namespace UnRechnung.ViewModels
{
  public partial class CustomersViewModel : ViewModelBase, IAsyncInitializable
  {
    [ObservableProperty]
    private int _selectedTableItemIndex = 0;
    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    public ObservableCollection<Customer> TableItems { get; set; }

    private readonly AppDbContext _dbContext;
    private readonly IFileDialogService _fileDialogService;
    private readonly INavigationHost _navigationHost;
    private readonly INotificationService _notificationService;

    /// <summary>Needed for the design view.</summary>
    public CustomersViewModel() { }

    public CustomersViewModel(AppDbContext dbContext, IFileDialogService fileDialogService, INavigationHost navigationHost, INotificationService notificationService)
    {
      _dbContext = dbContext;
      _fileDialogService = fileDialogService;
      _navigationHost = navigationHost;
      _notificationService = notificationService;
    }

    public async Task InitializeAsync()
    {
      TableItems = new ObservableCollection<Customer>(await _dbContext.Customers.Where(c => c.IsActive).ToListAsync());
    }

    [RelayCommand]
    private async Task AddCustomerAsync()
    {
      var item = await ShowCustomerDialogAsync("Kunde erstellen", null);

      // New customer was created
      if (item != null)
      {
        TableItems.Add(item);
        SelectedTableItemIndex = TableItems.Count - 1;
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, $"Neuer Kunde wurde erstellt: \"{item.Name}\"");
      }
    }

    [RelayCommand]
    private async Task ImportCustomersAsync()
    {
      // Open file dialog to chose csv file
      string inputPath = await _fileDialogService.OpenFileDialogAsync("Kundenliste importieren", FileDialogService.csvFileType, FilePickerFileTypes.All);

      // No path - no export
      if (string.IsNullOrEmpty(inputPath)) return;

      try
      {
        // Add items to products table
        using StreamReader reader = new StreamReader(inputPath);
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
          Delimiter = ";",
        };
        using CsvReader csv = new CsvReader(reader, config);
        {
          csv.Context.RegisterClassMap<CustomerCsvMap>();

          foreach (var record in csv.GetRecords<Customer>())
          {
            if (record.CustomerNumber == 0)
            {
              var numRange = _dbContext.NumberRanges.Where(n => n.Type == NumberRange.eType.Customer).First();

              if (numRange != null)
              {
                record.CustomerNumber = numRange.NextNumber;
                ++numRange.NextNumber;
              }
            }

            _dbContext.Customers.Add(record);
            TableItems.Add(record);
          }

          await _dbContext.SaveChangesAsync();

          _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, "Kundenimport erfolgreich.");
        }
      }
      catch (IOException e)
      {
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Error, $"Die Datei \"{inputPath}\" kann nicht geöffnet werden.");
      }
    }

    [RelayCommand]
    private async Task ExportCustomersAsync()
    {
      // TODO - Not implemented yet
    }

    [RelayCommand]
    private async Task EditItemAsync(Customer item)
    {
      var result = await ShowCustomerDialogAsync("Kunde bearbeiten", item);

      if (result != null)
      {
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, $"Änderungen wurden gespeichert: \"{item.Name}\"");
      }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(Customer item)
    {
      var mainWindow = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;

      if (mainWindow == null) return;

      var confirmationDialog = new UserDialogViewModel(UserDialogViewModel.eTitle.Warning, $"Möchten Sie den Kunden \"**{item.Name}**\" wirklich löschen?", UserDialogViewModel.eButtonTexts.NoYes);

      UserDialogViewModel.eResult result = await confirmationDialog.ShowDialogAsync(mainWindow);

      switch (result)
      {
        case UserDialogViewModel.eResult.Yes:
          {
            _dbContext.Customers.Remove(item);
            _dbContext.SaveChanges();
            TableItems.Remove(item);
          }
          break;

        case UserDialogViewModel.eResult.No:
        case UserDialogViewModel.eResult.Cancel:
        default:
          // Do nothing
          break;
      }
    }

    partial void OnCustomerSearchTextChanged(string value)
    {
      TableItems.Clear();

      // Search customer number
      if (int.TryParse(value, out var customerNumber))
      {
        foreach (var customer in _dbContext.Customers.Where(c => c.CustomerNumber.ToString().Contains(value)))
        {
          TableItems.Add(customer);
        }
      }
      else
      {
        // Search customer name
        foreach (var customer in _dbContext.Customers.Where(c => (c.Name.ToLower().Contains(value.ToLower())) || (c.Representative.ToLower().Contains(value.ToLower())) ))
        {
          TableItems.Add(customer);
        }
      }
    }

    private async Task<Customer?> ShowCustomerDialogAsync(string dialogTitle, Customer? item)
    {
      var mainWindow = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
      var result = item;

      if (mainWindow != null)
      {
        var vm = new CustomerDialogViewModel(dialogTitle, _dbContext, item);
        var dialog = new CustomerDialogView { DataContext = vm };

        vm.DialogWindow = dialog;

        _navigationHost.IsDeemphasized = true;
        result = await dialog.ShowDialog<Customer>(mainWindow);
        _navigationHost.IsDeemphasized = false;
      }

      return (result);
    }

    private void DbRemoveCustomer(long _customerNumber)
    {
      var customer = _dbContext.Customers
        .Include(c => c.Invoices)
        .FirstOrDefault(c => c.CustomerNumber == _customerNumber);

      if (customer != null)
      {
        // Check if the customer has any invoices
        if (customer.Invoices == null || !customer.Invoices.Any())
        {
          _dbContext.Customers.Remove(customer);
        }
        else
        {
          // Don't delete customer if there are any invoices
          // TODO: Dialog if the user really wants to delete a customer with existing invoices
          customer.Anonymize();
        }
      }
    }
  }
}
