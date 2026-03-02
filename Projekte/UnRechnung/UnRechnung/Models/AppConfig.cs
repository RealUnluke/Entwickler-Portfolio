using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnRechnung.Models
{
  public class AppConfig : INotifyPropertyChanged
  {
    public enum eTheme
    {
      Light,
      Dark
    }

    private CompanyConfig _companyConfig = new CompanyConfig();
    private eTheme _theme = eTheme.Light;

    public CompanyConfig CompanyConfig
    {
      get => _companyConfig;
      set { _companyConfig = value; OnPropertyChanged(); }
    }

    public eTheme Theme
    {
      get => _theme;
      set { _theme = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class CompanyConfig : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _street = string.Empty;
    private string _addressComplement = string.Empty;
    private string _postalCode = string.Empty;
    private string _city = string.Empty;
    private string _logoPath = string.Empty;

    public string Name
    {
      get => _name;
      set { _name = value; OnPropertyChanged(); }
    }
    public string Street
    {
      get => _street;
      set { _street = value; OnPropertyChanged(); }
    }
    public string AddressComplement
    {
      get => _addressComplement;
      set { _addressComplement = value; OnPropertyChanged(); }
    }
    public string PostalCode
    {
      get => _postalCode;
      set { _postalCode = value; OnPropertyChanged(); }
    }
    public string City
    {
      get => _city;
      set { _city = value; OnPropertyChanged(); }
    }
    public string LogoPath
    {
      get => _logoPath;
      set { _logoPath = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
