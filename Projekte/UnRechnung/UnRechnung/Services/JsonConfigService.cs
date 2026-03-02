using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnRechnung.Models;

namespace UnRechnung.Services
{
  /// <summary>File dialog service interface.</summary>
  public interface IJsonConfigService
  {
    /// <summary>Contains all of the apps configuration.</summary>
    AppConfig Config { get; }

    /// <summary>Loads the configuration from a config file.</summary>
    void Load();

    /// <summary>Saves the current configuration in a config file.</summary>
    Task SaveAsync();
  }

  public class JsonConfigService : IJsonConfigService
  {
    /// <summary>Contains all of the apps configuration.</summary>
    public AppConfig Config { get; private set; } = new AppConfig();

    private readonly string _configFilePath;
    private readonly string _imagesPath;

    public JsonConfigService()
    {
      // Get the platform-appropriate AppData folder
      var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      // Define a folder for the app inside AppData
      var myAppFolder = Path.Combine(appDataFolder, "UnRechnung");

      // Define a folder for config files
      var configFolder = Path.Combine(myAppFolder, "config");

      // Define a folder for asset files
      var assetsFolder = Path.Combine(myAppFolder, "assets");

      // Define a folder for image files
      _imagesPath = Path.Combine(assetsFolder, "images");

      // Make sure the directory exists (create if needed)
      Directory.CreateDirectory(_imagesPath);

      // Make sure the directory exists (create if needed)
      Directory.CreateDirectory(configFolder);

      // Define full path to the SQLite database file
      _configFilePath = Path.Combine(configFolder, "user_settings.json");
    }

    /// <summary>Loads the configuration from a config file.</summary>
    public void Load()
    {
      if (!File.Exists(_configFilePath))
      {
        Save(); // Write defaults
        return;
      }

      var json = File.ReadAllText(_configFilePath);
      Config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    /// <summary>Saves the current configuration in a config file.</summary>
    public void Save()
    {
      CopyLogoFile();
      var json = JsonSerializer.Serialize(Config);

      File.WriteAllText(_configFilePath, json);
    }

    /// <summary>Saves the current configuration in a config file.</summary>
    public async Task SaveAsync()
    {
      CopyLogoFile();
      var json = JsonSerializer.Serialize(Config);

      await File.WriteAllTextAsync(_configFilePath, json);
    }

    private void CopyLogoFile()
    {
      var logoPath = Path.Combine(_imagesPath, "company_image.png");

      if (logoPath == Config.CompanyConfig.LogoPath) return;

      File.Replace(Config.CompanyConfig.LogoPath, logoPath, null);

      Config.CompanyConfig.LogoPath = logoPath;
    }
  }
}
