using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using YouTube_Downloader.Models;
using System.Collections.Generic;
using System;

namespace YouTube_Downloader.ViewModels
{

  public partial class MainWindowViewModel : ViewModelBase
  {
    public enum eFormatType
    {
      Audio,
      Video
    }

    public enum eQualityType
    {
      Low = 1,
      Medium = 5,
      Best = 10
    }

    [ObservableProperty]
    private string? _url = "https://www.youtube.com/watch?v=zqOWV_pq9Zs";
    [ObservableProperty]
    private string? _log;
    [ObservableProperty]
    private string? _outputPath = "C:\\Users\\Admin\\Desktop\\newsic\\";
    [ObservableProperty]
    //private string? _format = "%year% - %album% - %artist% - %track% - %title%"; // Target format
    private string? _format = "%(title)s";

    [ObservableProperty]
    private eFormatType _selectedFormat = eFormatType.Audio;

    [ObservableProperty]
    private eQualityType _selectedQuality = eQualityType.Medium;

    public ObservableCollection<eFormatType> Formats { get; } = new() { eFormatType.Audio, eFormatType.Video };
    public ObservableCollection<eQualityType> Qualities { get; } = new() { eQualityType.Low, eQualityType.Medium, eQualityType.Best };

    public static FilePickerFileType AudioFileType { get; } = new("Audio Files")
    {
      Patterns = ["*.mp3"],
      AppleUniformTypeIdentifiers = ["public.audio"],
      MimeTypes = ["audio/*"]
    };

    public ObservableCollection<DownloadRecord> DownloadRecords { get; } = new ObservableCollection<DownloadRecord>();
    private Queue<DownloadRecord> _downloadQueue = new Queue<DownloadRecord>();

    [ObservableProperty]
    private bool _isDownloading = false;
    [ObservableProperty]
    private double _downloadProgress = 0;

    private bool _isProcessing = false;

    private static readonly Regex StatusRegex = new(@"\d+.\d+(?=%)", RegexOptions.Compiled);
    private static readonly Regex SpeedRegex = new(@"(\d+.\d+)([KMG])(?=iB\/s)", RegexOptions.Compiled);
    private static readonly Regex EtaRegex = new(@"\d+:\d+", RegexOptions.Compiled);
    private static readonly Regex TitleRegex = new(@"(?<=\\)[\w -]+(?=.mp4)", RegexOptions.Compiled);

    [RelayCommand]
    public async Task SelectFileAsync()
    {
      var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

      if (topLevel?.StorageProvider is { CanOpen: true } storageProvider)
      {
        var options = new FolderPickerOpenOptions
        {
          Title = "Save Path",
        };

        var files = await storageProvider.OpenFolderPickerAsync(options);

        if (files.Count > 0)
        {
          OutputPath = files[0].Path.LocalPath.ToString();
        }
      }
    }

    [RelayCommand]
    private void AddDownload()
    {
      var job = new DownloadRecord();
      _downloadQueue.Enqueue(job);
      DownloadRecords.Add(job);

      ProcessQueue();
    }

    private async void ProcessQueue()
    {
      if (_isProcessing)
      {
        return;
      }

      _isProcessing = true;

      while (_downloadQueue.Count > 0)
      {
        var job = _downloadQueue.Dequeue();
        await RunDownloadAsync(job);
      }

      _isProcessing = false;
    }

    private async Task RunDownloadAsync(DownloadRecord currentDownload)
    {
      IsDownloading = true;
      currentDownload.Status = 0.0m;

      await Task.Run(() =>
      {
        var psi = CreateProcessInfo(currentDownload);
        using var process = Process.Start(psi);

        if (process != null)
        {
          process.OutputDataReceived += (_, e) =>
          {
            if (e.Data != null)
            {
              ParseYtDlpOutput(currentDownload, e.Data);
            }
          };

          process.BeginOutputReadLine();
          process.WaitForExit();
        }

        currentDownload.Speed = 0.0m;
        currentDownload.Status = 100.0m;
      });
    }

    private ProcessStartInfo CreateProcessInfo(DownloadRecord currentDownload)
    {
      string arguments = "";

      if (SelectedFormat == eFormatType.Audio)
      {
        arguments = "-x --audio-format mp3 ";
      }

      arguments += $"--audio-quality {(int)SelectedQuality} " +
                   $"-o \"{OutputPath}/{Format}.%(ext)s\" {Url?.Trim()}";

      var psi = new ProcessStartInfo
      {
        FileName = "yt-dlp",
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      return (psi);
    }

    private void ParseYtDlpOutput(DownloadRecord currentDownload, string line)
    {
      if (!line.StartsWith("[download]")) return;

      decimal decVal = 0.0m;
      TimeSpan timeVal = TimeSpan.Zero;

      Dispatcher.UIThread.Post(() =>
      {
        if (TitleRegex.Match(line) is { Success: true } m)
        {
          currentDownload.Title = m.Value;
        }

        if (EtaRegex.Match(line) is { Success: true } e &&
            TimeSpan.TryParse($"00:{e.Value}", CultureInfo.InvariantCulture, out timeVal))
        {
          currentDownload.ETA = (timeVal.Minutes * 60) + timeVal.Seconds;
        }

        if (SpeedRegex.Match(line) is { Success: true } s)
        {
          char charVal = '0';

          if (char.TryParse(s.Groups[2].Value, out charVal) &&
              decimal.TryParse(s.Groups[1].Value, CultureInfo.InvariantCulture, out decVal))
          {
            // Convert GiB, MiB and KiB to KB
            switch (charVal)
            {
              case 'G': decVal = decVal * 8 * 1024 * 1024 * 1024 / 8 / 1000; break;
              case 'M': decVal = decVal * 8 * 1024 * 1024 / 8 / 1000; break;
              case 'K':
              default: decVal = decVal * 8 * 1024 / 8 / 1000; break;
            }
          }

          currentDownload.Speed = decVal;
        }

        if (StatusRegex.Match(line) is { Success: true } st &&
            decimal.TryParse(st.Value, CultureInfo.InvariantCulture, out decVal))
        {
          if (decVal < 100.0m)
          {
            currentDownload.Status = (decVal > currentDownload.Status) ? decVal : currentDownload.Status;
          }
        }
      });
    }
  }
}
