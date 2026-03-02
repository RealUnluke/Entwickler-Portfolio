using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace UnRechnung.ViewModels;

public partial class SnackbarViewModel : ViewModelBase
{
  public enum eSnackbarType
  {
    Info,
    Success,
    Warning,
    Error
  }

  [ObservableProperty]
  private string _message = string.Empty;
  [ObservableProperty]
  private bool _isHidden = true;
  [ObservableProperty]
  private IAsyncRelayCommand? _closeCommand;
  [ObservableProperty]
  private IBrush? _snackbarForeground;
  [ObservableProperty]
  private IBrush? _snackbarBackground;
  [ObservableProperty]
  private StreamGeometry? _snackbarIcon;
  [ObservableProperty]
  private double _snackbarProgress;
  [ObservableProperty]
  private double _snackbarProgressMax;

  private System.Timers.Timer _timer = new System.Timers.Timer();

  /// <summary>Needed for the design view.</summary>
  public SnackbarViewModel() { }

  public SnackbarViewModel(eSnackbarType snackbarType, string message, Func<SnackbarViewModel, Task> deleteAction)
  {
    Message = message;
    CloseCommand = new AsyncRelayCommand(() => deleteAction(this));

    switch (snackbarType)
    {
      case eSnackbarType.Success:
        {
          SnackbarBackground = (IBrush?)Application.Current!.FindResource("SuccessBackground");
          SnackbarForeground = (IBrush?)Application.Current!.FindResource("SuccessForeground");
          SnackbarIcon = (StreamGeometry?)Application.Current!.FindResource("success_icon");
        }
        break;
      case eSnackbarType.Warning:
        {
          SnackbarBackground = (IBrush?)Application.Current!.FindResource("WarningBackground");
          SnackbarForeground = (IBrush?)Application.Current!.FindResource("WarningForeground");
          SnackbarIcon = (StreamGeometry?)Application.Current!.FindResource("warning_icon");
        }
        break;
      case eSnackbarType.Error:
        {
          SnackbarBackground = (IBrush?)Application.Current!.FindResource("ErrorBackground");
          SnackbarForeground = (IBrush?)Application.Current!.FindResource("ErrorForeground");
          SnackbarIcon = (StreamGeometry?)Application.Current!.FindResource("error_icon");
        }
        break;
      default:
      case eSnackbarType.Info:
        {
          SnackbarBackground = (IBrush?)Application.Current!.FindResource("InfoBackground");
          SnackbarForeground = (IBrush?)Application.Current!.FindResource("InfoForeground");
          SnackbarIcon = (StreamGeometry?)Application.Current!.FindResource("info_icon");
        }
        break;
    }
  }

  public async Task StartAutoCloseAsync(Action<SnackbarViewModel> onClose, int durationMs = 5_000)
  {
    // Set progress bar value and its maximum to the duration in seconds
    SnackbarProgress = SnackbarProgressMax = (durationMs / 1_000) - 1;
  
    // Setting up timer
    _timer.Elapsed += TimerElapsed;
    _timer.AutoReset = true;
    _timer.Interval = 1_000;
    _timer.Start();

    // Wait before closing
    await Task.Delay(durationMs);

    // Start FadeOut animation and wait for completion
    IsHidden = true;
    await Task.Delay(300);

    // Close
    onClose(this);
  }

  private void TimerElapsed(object? sender, ElapsedEventArgs e)
  {
    --SnackbarProgress;

    if (SnackbarProgress <= 0)
    {
      _timer.Stop();
      _timer.Dispose();
    }
  }
}
