using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UnRechnung.Models;
using UnRechnung.Services;

namespace UnRechnung.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  public enum ePage
  {
    Dashboard,
    Products,
    Customer,
    InvoiceList,
    Settings
  }

  // ==============================================================================
  // MENU BAR
  // ==============================================================================
  [ObservableProperty]
  private ePage _currentPage = 0;


  // ==============================================================================
  // CURRENT PAGE
  // ==============================================================================
  [ObservableProperty]
  private INavigationHost _navigationHost;

  // ==============================================================================
  // NOTIFICATIONS
  // ==============================================================================
  [ObservableProperty]
  private int _selectedSnackbarTypeIndex = 0;

  public ObservableCollection<SnackbarViewModel> SnackbarNotifications { get; set; } = new ObservableCollection<SnackbarViewModel>();


  private readonly INavigationService _navigationService;
  private readonly IJsonConfigService _jsonConfigService;

  /// <summary>Needed for the design view.</summary>
  public MainWindowViewModel() { }

  public MainWindowViewModel(IJsonConfigService jsonConfigService, INavigationHost navigationHost, INavigationService navigationService)
  {
    _jsonConfigService = jsonConfigService;
    _navigationHost = navigationHost;
    _navigationService = navigationService;
  }

  public void Initialize()
  {
    _jsonConfigService.Load();
    _navigationService.NavigateTo<DashboardViewModel>();

    LoadThemeFromConfig();
  }

  private void LoadThemeFromConfig()
  {
    ThemeVariant newTheme;

    switch (_jsonConfigService.Config.Theme)
    {
      case AppConfig.eTheme.Dark: newTheme = ThemeVariant.Dark; break;
      default:
      case AppConfig.eTheme.Light: newTheme = ThemeVariant.Light; break;
    }

    if (Application.Current!.ActualThemeVariant != newTheme)
    {
      Application.Current!.RequestedThemeVariant = newTheme;
    }
  }

  // ==============================================================================
  // MENU BAR
  // ==============================================================================
  #region MenuBar
  partial void OnCurrentPageChanged(ePage value)
  {
    switch (value)
    {
      case ePage.Customer: _navigationService.NavigateTo<CustomersViewModel>(); break;
      case ePage.Products: _navigationService.NavigateTo<ProductsViewModel>(); break;
      case ePage.InvoiceList: _navigationService.NavigateTo<InvoiceListViewModel>(); break;
      case ePage.Settings: _navigationService.NavigateTo<SettingsViewModel>(); break;
      case ePage.Dashboard:
      default: _navigationService.NavigateTo<DashboardViewModel>(); break;
    }
  }
  #endregion
}
