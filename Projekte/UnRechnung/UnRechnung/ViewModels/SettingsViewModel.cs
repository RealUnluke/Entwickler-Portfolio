using Avalonia.Media.Imaging;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using UnRechnung.Data;
using UnRechnung.Interfaces;
using UnRechnung.Models;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using UnRechnung.Services;
using Avalonia.Styling;
using Avalonia;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UnRechnung.ViewModels
{
  public partial class SettingsViewModel : ViewModelBase, IAsyncInitializable
  {
    // ==============================================================================
    // FIRST TAB - USER SETTINGS
    // ==============================================================================
    [ObservableProperty]
    private AppConfig.eTheme _selectedThemeIndex = AppConfig.eTheme.Light;
    [ObservableProperty]
    private IBrush _themeIconColor;
    [ObservableProperty]
    private Bitmap? _companyLogo;
    [ObservableProperty]
    private string _addLogoButtonText = "Logo hinzufügen";
    [ObservableProperty]
    private bool _hasUnsavedChanges = false;
    [ObservableProperty]
    private bool _isLogoMissing = false;

    [ObservableProperty]
    private CompanyConfig _companyConfig;

    // ==============================================================================
    // SECOND TAB - NUMBER RANGES
    // ==============================================================================
    [ObservableProperty]
    private string _invoiceNumberPreview = string.Empty;
    [ObservableProperty]
    private string _invoiceNumberRangePrefix;
    [ObservableProperty]
    private string _invoiceNumberRangeFormat;
    [ObservableProperty]
    private long _invoiceNumberRangeNextNumber;
    [ObservableProperty]
    private int _minLengthIndex = 1;

    [ObservableProperty]
    private NumberRange? _customerNumberRange;
    [ObservableProperty]
    private NumberRange? _invoiceNumberRange;


    private readonly AppDbContext _dbContext;
    private readonly IFileDialogService _fileDialogService;
    private readonly IJsonConfigService _jsonConfigService;
    private readonly INotificationService _notificationService;

    /// <summary>Needed for the design view.</summary>
    public SettingsViewModel() { }

    public SettingsViewModel(AppDbContext dbContext, IFileDialogService fileDialogService, IJsonConfigService jsonConfigService, INotificationService notificationService)
    {
      _dbContext = dbContext;
      _fileDialogService = fileDialogService;
      _jsonConfigService = jsonConfigService;
      _notificationService = notificationService;
      _companyConfig = _jsonConfigService.Config.CompanyConfig;

      CompanyConfig.PropertyChanged += (_, _) => HasUnsavedChanges = true;

      SelectedThemeIndex = (Application.Current!.ActualThemeVariant == ThemeVariant.Light) ? AppConfig.eTheme.Light : AppConfig.eTheme.Dark;
      ThemeIconColor = (Application.Current!.ActualThemeVariant == ThemeVariant.Light) ? Brushes.Black : Brushes.White;
    }

    public async Task InitializeAsync()
    {
      CustomerNumberRange = await _dbContext.NumberRanges.Where(n => n.Type == NumberRange.eType.Customer).FirstOrDefaultAsync();
      InvoiceNumberRange = await _dbContext.NumberRanges.Where(n => n.Type == NumberRange.eType.Invoice).FirstOrDefaultAsync();
      InvoiceNumberRangePrefix = InvoiceNumberRange!.Prefix;
      InvoiceNumberRangeFormat = InvoiceNumberRange!.Format;
      InvoiceNumberRangeNextNumber = InvoiceNumberRange!.NextNumber;
      InvoiceNumberPreview = InvoiceNumberRange!.GetInvoiceNumberString();
      _jsonConfigService.Load();

      SetCompanyLogoInUi();
    }

    // ==============================================================================
    // FIRST TAB - USER SETTINGS
    // ==============================================================================
    #region Settings
    [RelayCommand]
    private async Task ChoseLogoPathAsync()
    {
      // Open file dialog to chose logo file path
      var inputPath = await _fileDialogService.OpenFileDialogAsync(FilePickerFileTypes.ImageAll, FilePickerFileTypes.All);

      // No path - no save
      if (string.IsNullOrEmpty(inputPath)) return;

      // Set logo from selected file path
      CompanyLogo = new Bitmap(inputPath);
      IsLogoMissing = false;

      // Write into config
      CompanyConfig.LogoPath = inputPath;
      HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
      _jsonConfigService.Config.CompanyConfig = CompanyConfig;
      await _jsonConfigService.SaveAsync();
      HasUnsavedChanges = false;

      _notificationService.ShowSnackbarNotification(SnackbarViewModel.eSnackbarType.Success, "Änderungen wurden gespeichert.");
    }

    partial void OnSelectedThemeIndexChanged(AppConfig.eTheme value)
    {
      ThemeVariant newTheme;

      switch (value)
      {
        case AppConfig.eTheme.Dark: newTheme = ThemeVariant.Dark; ThemeIconColor = Brushes.White; break;
        default:
        case AppConfig.eTheme.Light: newTheme = ThemeVariant.Light; ThemeIconColor = Brushes.Black; break;
      }

      if (Application.Current!.ActualThemeVariant != newTheme)
      {
        Application.Current!.RequestedThemeVariant = newTheme;
      }

      // Write into config
      _jsonConfigService.Config.Theme = value;
      HasUnsavedChanges = true;
    }

    private void SetCompanyLogoInUi()
    {
      if (File.Exists(CompanyConfig.LogoPath))
      {
        CompanyLogo = new Bitmap(CompanyConfig.LogoPath);
        IsLogoMissing = false;
      }
    }
    #endregion

    // ==============================================================================
    // SECOND TAB - NUMBER RANGES
    // ==============================================================================
    #region NumberRanges
    partial void OnMinLengthIndexChanged(int value)
    {
      if (InvoiceNumberRange != null)
      {
        // Add 3 to match index with possible selection
        InvoiceNumberRange.PaddingLength = value + 3;
        InvoiceNumberPreview = InvoiceNumberRange.GetInvoiceNumberString();
      }
    }

    partial void OnInvoiceNumberRangePrefixChanged(string value)
    {
      if (InvoiceNumberRange != null)
      {
        InvoiceNumberRange.Prefix = value;
        InvoiceNumberPreview = InvoiceNumberRange.GetInvoiceNumberString();
      }
    }

    partial void OnInvoiceNumberRangeFormatChanged(string value)
    {
      if (InvoiceNumberRange != null)
      {
        InvoiceNumberRange.Format = value;
        InvoiceNumberPreview = InvoiceNumberRange.GetInvoiceNumberString();
      }
    }

    partial void OnInvoiceNumberRangeNextNumberChanged(long value)
    {
      if (InvoiceNumberRange != null)
      {
        InvoiceNumberRange.NextNumber = value;
        InvoiceNumberPreview = InvoiceNumberRange.GetInvoiceNumberString();
      }
    }
    #endregion
  }
}
