using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace UnRechnung.Models
{
  public class Customer : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _representative = string.Empty;
    private string _street = string.Empty;
    private string _addressComplement = string.Empty;
    private string _postalCode = string.Empty;
    private string _city = string.Empty;
    private string _country = string.Empty;
    private string _contactNumber = string.Empty;
    private string _email = string.Empty;

    [Required]
    [Name("Kundennummer")]
    public long CustomerNumber { get; set; }

    [Required]
    [MaxLength(100)]
    [Name("Name")]
    public string Name
    {
      get => _name;
      set { _name = value; OnPropertyChanged(); }
    }

    [MaxLength(100)]
    [Name("Ansprechpartner")]
    public string Representative
    {
      get => _representative;
      set { _representative = value; OnPropertyChanged(); }
    }

    [Required]
    [MaxLength(100)]
    [Name("Straße")]
    public string Street
    {
      get => _street;
      set { _street = value; OnPropertyChanged(); }
    }

    [MaxLength(100)]
    [Name("Adresszusatz")]
    public string AddressComplement
    {
      get => _addressComplement;
      set { _addressComplement = value; OnPropertyChanged(); }
    }

    [Required]
    [MaxLength(10)]
    [Name("PLZ")]
    public string PostalCode
    {
      get => _postalCode;
      set
      {
        _postalCode = value;

        if (value != null && value.Length >= 4)
        {
          City = "TODO";// _postalCodes.GetCity(value);
        }

        OnPropertyChanged();
      }
    }

    [Required]
    [MaxLength(100)]
    [Name("Ort")]
    public string City
    {
      get => _city;
      set { _city = value; OnPropertyChanged(); }
    }

    [Required]
    [Name("Land")]
    public string Country
    {
      get => _country;
      set { _country = value; OnPropertyChanged(); }
    }

    [Phone]
    [Name("Nummer")]
    public string ContactNumber
    {
      get => _contactNumber;
      set { _contactNumber = value; OnPropertyChanged(); }
    }

    [EmailAddress]
    [Name("E-Mail")]
    public string Email
    {
      get => _email;
      set { _email = value; OnPropertyChanged(); }
    }

    [Name("Aktiv")]
    public bool IsActive { get; set; } = true;

    //public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Invoice>? Invoices { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
      return (Name);
    }

    public bool CopyFrom(Customer other)
    {
      bool RetOk = false;

      if (other != null)
      {
        Name = other.Name;
        Representative = other.Representative;
        Street = other.Street;
        AddressComplement = other.AddressComplement;
        PostalCode = other.PostalCode;
        City = other.City;
        Country = other.Country;
        ContactNumber = other.ContactNumber;
        Email = other.Email;
        IsActive = other.IsActive;
        Invoices = other.Invoices;

        RetOk = true;
      }

      return (RetOk);
    }

    public void Anonymize()
    {
      Name = "[gelöscht]";
      Representative = string.Empty;
      Street = "[gelöscht]";
      AddressComplement = string.Empty;
      PostalCode = "[gelöscht]";
      City = "[gelöscht]";
      Country = "[gelöscht]";
      ContactNumber = string.Empty;
      Email = string.Empty;
      IsActive = false;
    }
  }
}
