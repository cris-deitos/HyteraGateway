using System;
using System.IO;
using System.Text.Json;
using HyteraGateway.UI.Models;

namespace HyteraGateway.UI.Services;

/// <summary>
/// Service for loading and saving application configuration
/// </summary>
public class ConfigurationService
{
    private const string ConfigFileName = "hyteragateway-ui.json";
    private readonly string _configPath;

    /// <summary>
    /// Gets the current configuration
    /// </summary>
    public UIConfiguration Configuration { get; private set; }

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "HyteraGateway");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }
        
        _configPath = Path.Combine(appFolder, ConfigFileName);
        Configuration = new UIConfiguration();
        
        LoadConfiguration();
    }

    /// <summary>
    /// Load configuration from JSON file
    /// </summary>
    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<UIConfiguration>(json);
                
                if (config != null)
                {
                    Configuration = config;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            // Continue with default configuration
        }
    }

    /// <summary>
    /// Save configuration to JSON file
    /// </summary>
    public void SaveConfiguration()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(Configuration, options);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            throw;
        }
    }
}
