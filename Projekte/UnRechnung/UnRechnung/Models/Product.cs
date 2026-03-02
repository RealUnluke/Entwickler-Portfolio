using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace UnRechnung.Models
{
  public class Product : INotifyPropertyChanged
  {
    private string? _name = string.Empty;
    private string? _description = string.Empty;
    private string? _unit = string.Empty;
    private decimal _unitPrice = 0.0m;
    private decimal _taxes = 0.0m;

    [Ignore]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name
    {
      get => _name;
      set { _name = value; OnPropertyChanged(); }
    }

    [MaxLength(200)]
    public string? Description
    {
      get => _description;
      set { _description = value; OnPropertyChanged(); }
    }

    [MaxLength(100)]
    public string? Unit
    {
      get => _unit;
      set { _unit = value; OnPropertyChanged(); }
    }

    [Range(0, 999999)]
    public decimal UnitPrice
    {
      get => _unitPrice;
      set { _unitPrice = value; OnPropertyChanged(); }
    }

    [Range(0, 100)]
    public decimal Taxes
    {
      get => _taxes;
      set { _taxes = value; OnPropertyChanged(); }
    }

    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public override string ToString()
    {
      return (Name ?? string.Empty);
    }

    public bool CopyFrom(Product other)
    {
      bool RetOk = false;

      if (other != null)
      {
        Name = other.Name;
        Description = other.Description;
        Unit = other.Unit;
        UnitPrice = other.UnitPrice;
        Taxes = other.Taxes;

        RetOk = true;
      }

      return (RetOk);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
