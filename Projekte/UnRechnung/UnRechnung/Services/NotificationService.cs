using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UnRechnung.ViewModels;

namespace UnRechnung.Services
{
  /// <summary>Notification host service interface.</summary>
  public interface INotificationHost
  {
    MainWindowViewModel MainWindow { get; set; }
  }

  /// <summary>Notification host service interface.</summary>
  public partial class NotificationHost : ObservableObject, INotificationHost
  {
    [ObservableProperty]
    private MainWindowViewModel? _mainWindow;
  }

  /// <summary>Notification service interface.</summary>
  public interface INotificationService
  {
    /// <summary>Change the view model that will be displayed inside the main window.</summary>
    void ShowSnackbarNotification(SnackbarViewModel.eSnackbarType snackbarType, string message, int durationMs = 5_000);
  }

  /// <summary>
  /// Navigation service.<br/>
  /// Provides a central service which allows changing showing notifications
  /// that will be displayed inside the main window.
  /// </summary>
  public class NotificationService : INotificationService
  {
    private readonly INotificationHost _host;
    private readonly IServiceProvider _services;

    /// <summary>Constructor</summary>
    /// <param name="host">MainWindow as host for notifications.</param>
    /// <param name="services"></param>
    public NotificationService(INotificationHost host, IServiceProvider services)
    {
      _host = host;
      _services = services;

      _host.MainWindow = _services.GetRequiredService<MainWindowViewModel>();
    }

    /// <summary>Change the view model that will be displayed inside the main window.</summary>
    public void ShowSnackbarNotification(SnackbarViewModel.eSnackbarType snackbarType, string message, int durationMs = 5_000)
    {
      SnackbarViewModel notification = new SnackbarViewModel(snackbarType, message, CloseNotificationAsync);
      _host.MainWindow.SnackbarNotifications.Add(notification);

      // Show the notification after it appears in the VisualTree
      Avalonia.Threading.Dispatcher.UIThread.Post(() => notification.IsHidden = false);

      // Start auto closing
      _ = notification.StartAutoCloseAsync(_ =>
      {
        _host.MainWindow.SnackbarNotifications.Remove(notification);
      }, durationMs);
    }

    private async Task CloseNotificationAsync(SnackbarViewModel notification)
    {
      // Start FadeOut animation and wait for completion
      notification.IsHidden = true;
      await Task.Delay(300);

      // Remove notification from MainWindow
      _host.MainWindow.SnackbarNotifications.Remove(notification);
    }
  }
}
