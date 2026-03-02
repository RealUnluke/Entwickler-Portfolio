using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using UnRechnung.ViewModels;
using UnRechnung.Views;
using UnRechnung.Services;
using Microsoft.Extensions.DependencyInjection;
using UnRechnung.Data;

namespace UnRechnung;

public partial class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    // Register all the services needed for the application to run
    ServiceCollection collection = new ServiceCollection();

    // DbContext
    collection.AddDbContext<AppDbContext>();

    // ViewModels
    collection.AddSingleton<MainWindowViewModel>();
    collection.AddTransient<DashboardViewModel>();
    collection.AddTransient<ProductsViewModel>();
    collection.AddTransient<CustomersViewModel>();
    collection.AddTransient<InvoiceViewModel>();
    collection.AddTransient<InvoiceListViewModel>();
    collection.AddTransient<SettingsViewModel>();

    // Services
    collection.AddSingleton<IFileDialogService, FileDialogService>();
    collection.AddSingleton<INavigationHost, NavigationHost>();
    collection.AddSingleton<INavigationService, NavigationService>();
    collection.AddSingleton<INotificationHost, NotificationHost>();
    collection.AddSingleton<INotificationService, NotificationService>();
    collection.AddSingleton<IJsonConfigService, JsonConfigService>();
    collection.AddSingleton<IPdfService, PdfService>();
    collection.AddSingleton<IPostalCodeService, PostalCodeService>();

    // Creates a ServiceProvider containing services from the provided IServiceCollection
    var services = collection.BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
      // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
      DisableAvaloniaDataAnnotationValidation();

      var mainViewModel = services.GetRequiredService<MainWindowViewModel>();
      mainViewModel.Initialize();

      desktop.MainWindow = new MainWindow
      {
        DataContext = mainViewModel
      };

      // Set top level window for the file dialog service
      var fileDialogService = services.GetRequiredService<IFileDialogService>();
      fileDialogService.SetTopLevel(desktop.MainWindow);
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    {
      singleViewPlatform.MainView = new InvoiceView
      {
        DataContext = services.GetRequiredService<DashboardViewModel>()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void DisableAvaloniaDataAnnotationValidation()
  {
    // Get an array of plugins to remove
    var dataValidationPluginsToRemove =
        BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

    // remove each entry found
    foreach (var plugin in dataValidationPluginsToRemove)
    {
      BindingPlugins.DataValidators.Remove(plugin);
    }
  }
}