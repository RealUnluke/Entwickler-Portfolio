using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YouTube_Downloader.Models
{
  public class DownloadRecord : INotifyPropertyChanged
  {
    private string _title = string.Empty;
    private decimal _speed = 0.0m;
    private int _eta = 0;
    private decimal _status = -1.0m;

    public string Title
    {
      get => _title;
      set { _title = value; OnPropertyChanged(); }
    }

    public decimal Speed
    {
      get => _speed;
      set { _speed = value; OnPropertyChanged(); }
    }

    public int ETA
    {
      get => _eta;
      set { _eta = value; OnPropertyChanged(); }
    }

    public decimal Status
    {
      get => _status;
      set { _status = value; OnPropertyChanged(); }
    }

    public DownloadRecord()
    {
      Init();
    }

    public DownloadRecord(string title, decimal speed, int eta, decimal status)
    {
      Title = title;
      Speed = speed;
      ETA = eta;
      Status = status;
    }

    private void Init()
    {
      Title = _title;
      Speed = _speed;
      ETA = _eta;
      Status = _status;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
