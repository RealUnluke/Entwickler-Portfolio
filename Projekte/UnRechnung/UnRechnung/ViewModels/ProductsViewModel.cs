using Avalonia.Controls.ApplicationLifetimes;
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
using UnRechnung.Views;

namespace UnRechnung.ViewModels
{
  public partial class ProductsViewModel : ViewModelBase, IAsyncInitializable
  {
    [ObservableProperty]
    private int _selectedTableItemIndex = 0;
    [ObservableProperty]
    private string _productSearchText = string.Empty;

    public ObservableCollection<Product> TableItems { get; set; }

    private readonly AppDbContext _dbContext;
    private readonly IFileDialogService _fileDialogService;
    private readonly INavigationHost _navigationHost;
    private readonly INotificationService _notificationService;

    /// <summary>Needed for the design view.</summary>
    public ProductsViewModel() { }

    public ProductsViewModel(AppDbContext dbContext, IFileDialogService fileDialogService, INavigationHost navigationHost, INotificationService notificationService)
    {
      _dbContext = dbContext;
      _fileDialogService = fileDialogService;
      _navigationHost = navigationHost;
      _notificationService = notificationService;
    }

    public async Task InitializeAsync()
    {
      TableItems = new ObservableCollection<Product>(await _dbContext.Products.ToListAsync());
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
      var item = await ShowProductDialogAsync("Produkt erstellen", null);

      // New product was created
      if (item != null)
      {
        TableItems.Add(item);
        SelectedTableItemIndex = TableItems.Count - 1;
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, $"Neues Produkt wurde erstellt: \"{item.Name ?? string.Empty}\"");
      }
    }

    [RelayCommand]
    private async Task EditItemAsync(Product item)
    {
      var result = await ShowProductDialogAsync("Produkt bearbeiten", item);

      if (result != null)
      {
        _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, $"Änderungen wurden gespeichert: \"{item.Name ?? string.Empty}\"");
      }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(Product item)
    {
      var mainWindow = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;

      if (mainWindow == null) return;

      var confirmationDialog = new UserDialogViewModel(UserDialogViewModel.eTitle.Warning, $"Möchten Sie das Produkt \"**{item.Name ?? string.Empty}**\" wirklich löschen?", UserDialogViewModel.eButtonTexts.NoYes);

      UserDialogViewModel.eResult result = await confirmationDialog.ShowDialogAsync(mainWindow);

      switch (result)
      {
        case UserDialogViewModel.eResult.Yes:
          {
            _dbContext.Products.Remove(item);
            _dbContext.SaveChanges();
            TableItems.Remove(item);
            _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, $"\"{item.Name ?? string.Empty}\" wurde gelöscht.");
          }
          break;

        case UserDialogViewModel.eResult.No:
        case UserDialogViewModel.eResult.Cancel:
        default:
          // Do nothing
          break;
      }
    }

    [RelayCommand]
    private async Task ImportProductsAsync()
    {
      /* TODO - Not implemented yet
      // Open file dialog to chose csv file
      string inputPath = await _fileDialogService.OpenFileDialogAsync("Produktliste importieren", FileDialogService.csvFileType, FilePickerFileTypes.All);

      // No path - no export
      if (string.IsNullOrEmpty(inputPath)) return;

      // Add items to products table
      using StreamReader reader = new StreamReader(inputPath);
      CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        Delimiter = ";",
      };
      using CsvReader csv = new CsvReader(reader, config);
      {
        foreach (var record in csv.GetRecords<Product>())
        {
          TableItems.Add(record);
        }
      }
      */
    }

    [RelayCommand]
    private async Task ExportProductsAsync()
    {
      /* TODO - Not implemented yet
      // No products - no export
      if (!TableItems.Any()) return;

      // Save file dialog to chose destination
      string outputPath = await _fileDialogService.SaveFileDialogAsync("Datei speichern", ".csv", "products_list", FileDialogService.csvFileType, FilePickerFileTypes.All);

      // No path - no export
      if (string.IsNullOrEmpty(outputPath)) return;

      using StreamWriter writer = new StreamWriter(outputPath);
      CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        Delimiter = ";",
      };
      using CsvWriter csv = new CsvWriter(writer, config);
      {
        csv.WriteRecords(TableItems);
      }
      */
    }

    partial void OnProductSearchTextChanged(string value)
    {
      TableItems.Clear();

      foreach (var product in _dbContext.Products.Where(p => (!string.IsNullOrEmpty(p.Name)) && (p.Name.ToLower().Contains(value.ToLower()))))
      {
        TableItems.Add(product);
      }
    }

    private async Task<Product?> ShowProductDialogAsync(string dialogTitle, Product? item)
    {
      var mainWindow = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
      var result = item;

      if (mainWindow != null)
      {
        var vm = new ProductDialogViewModel(dialogTitle, _dbContext, item);
        var dialog = new ProductDialogView { DataContext = vm };

        vm.DialogWindow = dialog;

        _navigationHost.IsDeemphasized = true;
        result = await dialog.ShowDialog<Product>(mainWindow);
        _navigationHost.IsDeemphasized = false;
      }

      return (result);
    }
  }
}
