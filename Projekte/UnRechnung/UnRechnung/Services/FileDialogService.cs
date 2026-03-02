using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UnRechnung.Services
{
  /// <summary>File dialog service interface.</summary>
  public interface IFileDialogService
  {
    /// <summary>
    /// Set the top level window to enable the creation of file dialogs.</br>
    /// This method needs to be called manually after creating the main window in App.axaml.cs.
    /// </summary>
    /// <param name="topLevel">Main window.</param>
    void SetTopLevel(TopLevel topLevel);

    /// <summary>Opens an open file dialog.</summary>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected file.</returns>
    Task<string> OpenFileDialogAsync(params FilePickerFileType[] fileTypes);

    /// <summary>Opens an open file dialog.</summary>
    /// <param name="title">Text that appears in the title bar of the picker.</param>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected file.</returns>
    Task<string> OpenFileDialogAsync(string title, params FilePickerFileType[] fileTypes);

    /// <summary>Opens a save file dialog.</summary>
    /// <param name="title">Text that appears in the title bar of the picker.</param>
    /// <param name="defaultExtension">Default extension to be used to save the file.</param>
    /// <param name="suggestedFileName">File name that the file picker suggests to the user.</param>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected destination.</returns>
    Task<string> SaveFileDialogAsync(string title, string defaultExtension, string suggestedFileName, params FilePickerFileType[] fileTypes);
  }

  public class FileDialogService : IFileDialogService
  {
    /// <summary>CSV file type.</summary>
    public static FilePickerFileType csvFileType { get; } = new FilePickerFileType("CSV-Datei")
    {
      Patterns = ["*.csv"],
      AppleUniformTypeIdentifiers = ["public.csv"],
      MimeTypes = ["csv"]
    };

    private TopLevel _topLevel;

    /// <summary>
    /// Set the top level window to enable the creation of file dialogs.</br>
    /// This method needs to be called manually after creating the main window in App.axaml.cs.
    /// </summary>
    /// <param name="topLevel">Main window.</param>
    public void SetTopLevel(TopLevel topLevel)
    {
      _topLevel = topLevel;
    }

    /// <summary>Opens an open file dialog.</summary>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected file.</returns>
    public Task<string> OpenFileDialogAsync(params FilePickerFileType[] fileTypes) => OpenFileDialogAsync("Datei öffnen", fileTypes);

    /// <summary>Opens an open file dialog.</summary>
    /// <param name="title">Text that appears in the title bar of the picker.</param>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected file.</returns>
    public async Task<string> OpenFileDialogAsync(string title, params FilePickerFileType[] fileTypes)
    {
      string openPath = string.Empty;

      if (_topLevel.StorageProvider is { CanOpen: true } storageProvider)
      {
        var options = new FilePickerOpenOptions
        {
          Title = title,
          FileTypeFilter = fileTypes
        };

        var files = await storageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0)
        {
          openPath = files.First().TryGetLocalPath() ?? string.Empty;
        }
      }

      return (openPath);
    }

    /// <summary>Opens a save file dialog.</summary>
    /// <param name="title">Text that appears in the title bar of the picker.</param>
    /// <param name="defaultExtension">Default extension to be used to save the file.</param>
    /// <param name="suggestedFileName">File name that the file picker suggests to the user.</param>
    /// <param name="fileTypes">Array of preferred file types for the file picker.</param>
    /// <returns>Path string to the selected destination.</returns>
    public async Task<string> SaveFileDialogAsync(string title, string defaultExtension, string suggestedFileName, params FilePickerFileType[] fileTypes)
    {
      string savePath = string.Empty;

      if (_topLevel.StorageProvider is { CanOpen: true } storageProvider)
      {
        var options = new FilePickerSaveOptions
        {
          Title = title,
          DefaultExtension = defaultExtension,
          SuggestedFileName = suggestedFileName,
          FileTypeChoices = fileTypes
        };

        var files = await storageProvider.SaveFilePickerAsync(options);
        if (files != null)
        {
          savePath = files.Path.LocalPath.ToString();
        }
      }

      return (savePath);
    }
  }
}
