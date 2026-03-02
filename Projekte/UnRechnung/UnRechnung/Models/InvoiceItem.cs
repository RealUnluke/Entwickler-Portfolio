using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace UnRechnung.Models
{
  public class InvoiceItem : INotifyPropertyChanged
  {
    private int _position = 1;
    private Product? _product;
    private string? _name = string.Empty;
    private string? _description = string.Empty;
    private string? _unit = string.Empty;
    private decimal _quantity = 0.0m;
    private decimal _unitPrice = 0.0m;
    private decimal _discount = 0.0m;
    private decimal _taxes = 19.0m;
    private decimal _total = 0.0m;

    public int Id { get; set; }
    [Required]
    public int Position
    {
      get => _position;
      set { _position = value; OnPropertyChanged(); }
    }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }

    public int? ProductId { get; set; }
    public Product? Product
    {
      get => _product;
      set
      {
        _product = value;

        if (_product != null)
        {
          Name = _product.Name;
          Description = _product.Description;
          Unit = _product.Unit;
          UnitPrice = _product.UnitPrice;
          Taxes = _product.Taxes;
        }

        OnPropertyChanged();
      }
    }

    [MaxLength(200)]
    public string? Name
    {
      get => _name;
      set { _name = value; OnPropertyChanged(); }
    }

    [MaxLength(200)]
    public string? Description
    {
      get => _description;
      set { _description = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDescription)); }
    }

    [MaxLength(100)]
    public string? Unit
    {
      get => _unit;
      set { _unit = value; OnPropertyChanged(); }
    }

    [Required]
    public decimal Quantity
    {
      get => _quantity;
      set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Total)); }
    }

    [Range(0, 999999)]
    public decimal UnitPrice
    {
      get => _unitPrice;
      set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); OnPropertyChanged(nameof(Total)); }
    }

    [Range(0, 100)]
    public decimal Discount
    {
      get => _discount;
      set { _discount = value; OnPropertyChanged(nameof(Discount)); OnPropertyChanged(nameof(Total)); }
    }

    [Range(0, 100)]
    public decimal Taxes
    {
      get => _taxes;
      set { _taxes = value; OnPropertyChanged(); }
    }

    [Range(0, 999999)]
    public decimal Total
    {
      get { return ((Quantity * UnitPrice) * (1.00m + (Taxes / 100.00m)) * (1.00m - (Discount / 100.00m))); }
      set { _total = value; OnPropertyChanged(); }
    }

    // Needed to tell the UI to hide empty descriptions
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool CopyFrom(InvoiceItem other)
    {
      bool RetOk = false;

      if (other != null)
      {
        Position = other.Position;
        Description = other.Description;
        Unit = other.Unit;
        Quantity = other.Quantity;
        UnitPrice = other.UnitPrice;
        Discount = other.Discount;
        Taxes = other.Taxes;
        Total = other.Total;

        RetOk = true;
      }

      return (RetOk);
    }
  }
}
