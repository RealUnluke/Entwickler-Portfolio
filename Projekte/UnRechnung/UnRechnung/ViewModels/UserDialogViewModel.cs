using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using UnRechnung.Views;

namespace UnRechnung.ViewModels;

public partial class UserDialogViewModel : ViewModelBase
{
  public enum eTitle
  {
    Warning,
    Information,
    Error
  }

  public enum eButtonTexts
  {
    NoYes,
    OK
  }

  public enum eResult
  {
    Yes,
    No,
    Cancel
  }

  [ObservableProperty]
  private string _title = "Achtung";
  [ObservableProperty]
  private InlineCollection _message;
  //[ObservableProperty]
  //private string _message = "Sind Sie sicher?";
  [ObservableProperty]
  private string _noText = "Nein";
  [ObservableProperty]
  private string _yesText = "Ja";
  [ObservableProperty]
  private bool _showYes = true;

  private Window? _dialogWindow;
  private TaskCompletionSource<eResult>? _tcs;

  /// <summary>Needed for the design view.</summary>
  public UserDialogViewModel() { }

  public UserDialogViewModel(eTitle title, string message, eButtonTexts buttonTexts)
  {
    switch (title)
    {
      case eTitle.Information: Title = "Information"; break;
      case eTitle.Error: Title = "Fehler"; break;
      default:
      case eTitle.Warning: Title = "Achtung"; break;
    }

    switch (buttonTexts)
    {
      case eButtonTexts.OK:
        {
          NoText = "Ok";
        }
        break;

      default:
      case eButtonTexts.NoYes:
        {
          NoText = "Nein";
          YesText = "Ja";
        }
        break;
    }

    Message = ParseSimpleMarkdown(message);
    ShowYes = buttonTexts != eButtonTexts.OK;
  }

  public Task<eResult> ShowDialogAsync(Window owner)
  {
    _tcs = new TaskCompletionSource<eResult>();

    var dialog = new UserDialogView
    {
      DataContext = this
    };
    _dialogWindow = dialog;

    // Handle X button or Alt+F4
    dialog.Closed += (_, _) =>
    {
      if (!_tcs.Task.IsCompleted)
      {
        // User closed the dialog without choosing Yes/No
        // Cancel closing
        _tcs.SetResult(eResult.Cancel);
      }
    };

    dialog.ShowDialog(owner);

    return (_tcs.Task);
  }

  [RelayCommand]
  private void Yes()
  {
    Close(eResult.Yes);
  }

  [RelayCommand]
  private void No()
  {
    Close(eResult.No);
  }

  private void Close(eResult result)
  {
    _tcs?.SetResult(result);
    _dialogWindow?.Close();
  }

  private InlineCollection ParseSimpleMarkdown(string text)
  {
    var inlines = new InlineCollection();
    var parts = text.Split("**");

    for (int i = 0; i < parts.Length; i++)
    {
      inlines.Add(new Run(parts[i])
      {
        FontWeight = i % 2 == 1 ? FontWeight.Bold : FontWeight.Normal
      });
    }

    return (inlines);
  }
}
