using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Models;

namespace UnRechnung.ViewModels;

public partial class CustomerDialogViewModel : ViewModelBase
{
  public Action? CloseAction { get; set; }
  public Window? DialogWindow = null;

  [ObservableProperty]
  private string? _dialogTitle = "Kunde";
  [ObservableProperty]
  private Customer? _customerPending;

  private readonly Customer _customer;
  private readonly bool _isNew;

  private readonly AppDbContext _dbContext;

  /// <summary>Needed for the design view.</summary>
  public CustomerDialogViewModel() { }

  public CustomerDialogViewModel(string dialogTitle, AppDbContext dbContext, Customer? customer)
  {
    DialogTitle = dialogTitle;
    _dbContext = dbContext;
    _isNew = (customer == null);
    _customer = customer ?? new Customer();

    using var tempDbContext = new AppDbContext();
    CustomerPending = tempDbContext.Customers.FirstOrDefault(c => c.CustomerNumber == _customer.CustomerNumber) ?? _customer;
  }

  [RelayCommand]
  private void SaveChanges()
  {
    ExecuteSaveChanges();
    DialogWindow?.Close(_customer);
  }

  [RelayCommand]
  private async Task AbortChangesAsync()
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
      NumberRange customerNumberRange = _dbContext.NumberRanges.Where(n => (n.Type == NumberRange.eType.Customer)).First();
      _customer.CustomerNumber = customerNumberRange.NextNumber;
      ++customerNumberRange.NextNumber;

      _dbContext.Customers.Add(_customer);
    }
    else
    {
      _customer.CopyFrom(CustomerPending!);
    }

    _dbContext.SaveChanges();
  }
}
