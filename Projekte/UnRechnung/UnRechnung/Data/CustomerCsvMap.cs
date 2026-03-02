using CsvHelper.Configuration;
using UnRechnung.Models;

namespace UnRechnung.Data
{
  public sealed class CustomerCsvMap : ClassMap<Customer>
  {
    public CustomerCsvMap()
    {
      // Required
      Map(m => m.Name).Name("Name");
      Map(m => m.Street).Name("Straße");
      Map(m => m.PostalCode).Name("PLZ");
      Map(m => m.City).Name("Ort");
      Map(m => m.Country).Name("Land");

      // Optional
      Map(m => m.Representative).Name("Ansprechpartner").Optional();
      Map(m => m.AddressComplement).Name("Adresszusatz").Optional();
      Map(m => m.ContactNumber).Name("Nummer").Optional();
      Map(m => m.Email).Name("E-Mail").Optional();

      // Ignore
      Map(m => m.CustomerNumber).Ignore();
      Map(m => m.IsActive).Ignore();
    }
  }
}
