using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnRechnung.Interfaces;
using UnRechnung.ViewModels;

namespace UnRechnung.Services
{
  /// <summary>Navigation host service interface.</summary>
  public interface INavigationHost
  {
    ViewModelBase CurrentPage { get; set; }
    bool IsDeemphasized { get; set; }
  }

  /// <summary>Navigation host service interface.</summary>
  public partial class NavigationHost : ObservableObject, INavigationHost
  {
    [ObservableProperty]
    private ViewModelBase? _currentPage;
    [ObservableProperty]
    private bool _isDeemphasized;
  }

  /// <summary>Navigation service interface.</summary>
  public interface INavigationService
  {
    /// <summary>Change the view model that will be displayed inside the main window.</summary>
    void NavigateTo<T>() where T : ViewModelBase;
  }

  /// <summary>
  /// Navigation service.<br/>
  /// Provides a central service which allows changing the view model
  /// that will be displayed inside the main window.
  /// </summary>
  public class NavigationService : INavigationService
  {
    private readonly INavigationHost _host;
    private readonly IServiceProvider _services;

    /// <summary>Constructor</summary>
    /// <param name="host">MainWindow as host for navigation.</param>
    /// <param name="services"></param>
    public NavigationService(INavigationHost host, IServiceProvider services)
    {
      _host = host;
      _services = services;
    }

    /// <summary>Change the view model that will be displayed inside the main window.</summary>
    public void NavigateTo<T>() where T : ViewModelBase
    {
      var vm = _services.GetRequiredService<T>();

      if (vm is IAsyncInitializable asyncVm)
      {
        _ = asyncVm.InitializeAsync();
      }

      _host.CurrentPage = vm;
    }
  }
}
