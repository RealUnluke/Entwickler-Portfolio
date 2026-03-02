using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Models;

namespace UnRechnung.ViewModels;

public partial class ProductDialogViewModel : ViewModelBase
{
  public Window? DialogWindow = null;

  [ObservableProperty]
  private string? _dialogTitle = "Produkt";
  [ObservableProperty]
  private Product _productPending;

  private readonly Product _product;
  private readonly bool _isNew;

  private readonly AppDbContext _dbContext;

  /// <summary>Needed for the design view.</summary>
  public ProductDialogViewModel() { }

  public ProductDialogViewModel(string dialogTitle, AppDbContext dbContext, Product? product)
  {
    DialogTitle = dialogTitle;
    _dbContext = dbContext;
    _isNew = (product == null);
    _product = product ?? new Product();

    using var tempDbContext = new AppDbContext();
    ProductPending = tempDbContext.Products.FirstOrDefault(p => p.Id == _product.Id) ?? _product;
  }

  [RelayCommand]
  public void SaveChanges()
  {
    ExecuteSaveChanges();
    DialogWindow?.Close(_product);
  }

  [RelayCommand]
  public async Task AbortChangesAsync()
  {
    await ShowConfirmationDialogAsync();
  }

  private async Task ShowConfirmationDialogAsync()
  {
    if (DialogWindow != null)
    {
      var confirmationDialog = new UserDialogViewModel(UserDialogViewModel.eTitle.Warning, "Möchten Sie Ihre vorgenommenen Änderungen speichern?", UserDialogViewModel.eButtonTexts.NoYes);

      UserDialogViewModel.eResult result = await confirmationDialog.ShowDialogAsync(DialogWindow);

      switch (result)
      {
        case UserDialogViewModel.eResult.Yes:
          {
            SaveChanges();
          }
          break;

        case UserDialogViewModel.eResult.No:
          {
            DialogWindow?.Close();
          }
          break;

        case UserDialogViewModel.eResult.Cancel:
        default:
          // Do nothing and stay open
          break;
      }
    }
  }

  private void ExecuteSaveChanges()
  {
    if (_isNew)
    {
      _dbContext.Products.Add(ProductPending);
    }
    else
    {
      _product.CopyFrom(ProductPending);
    }

    _dbContext.SaveChanges();
  }
}
