using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace UnRechnung.Services
{
  /// <summary>Holds a postal code and the corresponding city.</summary>
  public class PostalCodeRecord
  {
    /// <summary>Postal code.</summary>
    public string PostalCode { get; set; } = string.Empty;
    /// <summary>Corresponding city.</summary>
    public string City { get; set; } = string.Empty;
  }

  /// <summary>Postal code service interface.</summary>
  public interface IPostalCodeService
  {
    /// <summary>Convenience method to find a city using its postal code.</summary>
    /// <param name="postalCode">Postal code.</param>
    /// <returns>Corresponding city to <paramref name="postalCode"/>.</returns>
    string GetCity(string postalCode);
  }

  /// <summary>
  /// Provides a service to find the corresponding city to a postal code in germany.<br/>
  /// This class uses a CSV file that contains all postal codes in germany.
  /// </summary>
  public class PostalCodeService : IPostalCodeService
  {
    /// <summary>Contains all german postal codes as the key to find their corresponding city.</summary>
    private Dictionary<string, string> PostalCodeToCityMap { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Constructor.</br>
    /// Map the postal codes to their corresponding city on construction of this class.
    /// </summary>
    public PostalCodeService()
    {
      string resourceName = "UnRechnung.Resources.de_postalcodes.csv";
      Assembly assembly = Assembly.GetExecutingAssembly();

      // Open resource stream
      using Stream stream = assembly.GetManifestResourceStream(resourceName)
          ?? throw new FileNotFoundException($"Ressource '{resourceName}' not found.");

      using StreamReader reader = new StreamReader(stream);
      CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        Delimiter = ";"
      };
      using CsvReader csv = new CsvReader(reader, config);

      foreach (var record in csv.GetRecords<PostalCodeRecord>())
      {
        if (!string.IsNullOrWhiteSpace(record.PostalCode) && !string.IsNullOrWhiteSpace(record.City))
        {
          PostalCodeToCityMap[record.PostalCode.Trim()] = record.City.Trim();
        }
      }
    }

    /// <summary>Convenience method to find a city using its postal code.</summary>
    /// <param name="postalCode">Postal code.</param>
    /// <returns>Corresponding city to <paramref name="postalCode"/>.</returns>
    public string GetCity(string postalCode) => PostalCodeToCityMap.TryGetValue(postalCode, out string? city) ? city : string.Empty;
  }
}
