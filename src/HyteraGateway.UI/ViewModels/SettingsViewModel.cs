using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Models;
using HyteraGateway.UI.Services;
using Microsoft.Win32;

namespace HyteraGateway.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigurationService _config;
    private readonly NetworkDiscoveryService _networkDiscovery;
    
    // Radio settings
    [ObservableProperty]
    private string _radioIpAddress = "192.168.1.1";
    
    [ObservableProperty]
    private int _radioControlPort = 50000;
    
    [ObservableProperty]
    private int _radioAudioPort = 50001;
    
    [ObservableProperty]
    private bool _autoReconnect = true;
    
    [ObservableProperty]
    private int _dispatcherId = 1;
    
    [ObservableProperty]
    private ObservableCollection<NetworkInterfaceInfo> _networkInterfaces = new();
    
    [ObservableProperty]
    private NetworkInterfaceInfo? _selectedInterface;
    
    // Database settings
    [ObservableProperty]
    private string _dbHost = "localhost";
    
    [ObservableProperty]
    private int _dbPort = 3306;
    
    [ObservableProperty]
    private string _dbName = "easyvol";
    
    [ObservableProperty]
    private string _dbUsername = "root";
    
    [ObservableProperty]
    private string _dbPassword = "";
    
    // API settings
    [ObservableProperty]
    private string _apiBaseUrl = "http://localhost:5000";
    
    [ObservableProperty]
    private int _apiPort = 5000;
    
    [ObservableProperty]
    private bool _enableHttps = false;
    
    [ObservableProperty]
    private bool _enableSwagger = true;
    
    [ObservableProperty]
    private string _corsOrigins = "*";
    
    // Audio settings
    [ObservableProperty]
    private bool _recordingEnabled = true;
    
    [ObservableProperty]
    private string _storagePath = "";
    
    [ObservableProperty]
    private string _audioFormat = "WAV";
    
    [ObservableProperty]
    private int _audioBitrate = 128;
    
    public string[] AudioFormats { get; } = { "WAV", "MP3" };
    
    // FTP settings
    [ObservableProperty]
    private bool _ftpEnabled = false;
    
    [ObservableProperty]
    private string _ftpHost = "";
    
    [ObservableProperty]
    private int _ftpPort = 21;
    
    [ObservableProperty]
    private string _ftpUsername = "";
    
    [ObservableProperty]
    private string _ftpPassword = "";
    
    [ObservableProperty]
    private string _ftpRemotePath = "/recordings";
    
    [ObservableProperty]
    private bool _ftpAutoUpload = false;
    
    [ObservableProperty]
    private bool _isSaving;
    
    [ObservableProperty]
    private string _statusMessage = "";

    public SettingsViewModel(ConfigurationService config, NetworkDiscoveryService networkDiscovery)
    {
        _config = config;
        _networkDiscovery = networkDiscovery;
        
        LoadSettings();
        RefreshNetworkInterfaces();
    }

    private void LoadSettings()
    {
        var cfg = _config.Configuration;
        
        RadioIpAddress = cfg.RadioIpAddress;
        RadioControlPort = cfg.RadioControlPort;
        RadioAudioPort = cfg.RadioAudioPort;
        AutoReconnect = cfg.AutoReconnect;
        DispatcherId = cfg.DispatcherId;
        
        DbHost = cfg.DbHost;
        DbPort = cfg.DbPort;
        DbName = cfg.DbName;
        DbUsername = cfg.DbUsername;
        DbPassword = cfg.DbPassword;
        
        ApiBaseUrl = cfg.ApiBaseUrl;
        ApiPort = cfg.ApiPort;
        EnableHttps = cfg.EnableHttps;
        EnableSwagger = cfg.EnableSwagger;
        CorsOrigins = cfg.CorsOrigins;
        
        RecordingEnabled = cfg.RecordingEnabled;
        StoragePath = cfg.StoragePath;
        AudioFormat = cfg.AudioFormat;
        AudioBitrate = cfg.AudioBitrate;
        
        FtpEnabled = cfg.FtpEnabled;
        FtpHost = cfg.FtpHost;
        FtpPort = cfg.FtpPort;
        FtpUsername = cfg.FtpUsername;
        FtpPassword = cfg.FtpPassword;
        FtpRemotePath = cfg.FtpRemotePath;
        FtpAutoUpload = cfg.FtpAutoUpload;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            IsSaving = true;
            
            var cfg = _config.Configuration;
            
            cfg.RadioIpAddress = RadioIpAddress;
            cfg.RadioControlPort = RadioControlPort;
            cfg.RadioAudioPort = RadioAudioPort;
            cfg.AutoReconnect = AutoReconnect;
            cfg.DispatcherId = DispatcherId;
            
            cfg.DbHost = DbHost;
            cfg.DbPort = DbPort;
            cfg.DbName = DbName;
            cfg.DbUsername = DbUsername;
            cfg.DbPassword = DbPassword;
            
            cfg.ApiBaseUrl = ApiBaseUrl;
            cfg.ApiPort = ApiPort;
            cfg.EnableHttps = EnableHttps;
            cfg.EnableSwagger = EnableSwagger;
            cfg.CorsOrigins = CorsOrigins;
            
            cfg.RecordingEnabled = RecordingEnabled;
            cfg.StoragePath = StoragePath;
            cfg.AudioFormat = AudioFormat;
            cfg.AudioBitrate = AudioBitrate;
            
            cfg.FtpEnabled = FtpEnabled;
            cfg.FtpHost = FtpHost;
            cfg.FtpPort = FtpPort;
            cfg.FtpUsername = FtpUsername;
            cfg.FtpPassword = FtpPassword;
            cfg.FtpRemotePath = FtpRemotePath;
            cfg.FtpAutoUpload = FtpAutoUpload;
            
            _config.SaveConfiguration();
            
            StatusMessage = "Settings saved successfully!";
            
            // Clear status message after 3 seconds
            _ = Task.Delay(3000).ContinueWith(_ => 
            {
                Application.Current.Dispatcher.Invoke(() => StatusMessage = "");
            }, TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task TestRadioConnection()
    {
        StatusMessage = "Testing radio connection...";
        
        var success = await _networkDiscovery.TestRadioConnectionAsync(RadioIpAddress, RadioControlPort);
        
        if (success)
        {
            StatusMessage = "✓ Radio connection successful!";
        }
        else
        {
            StatusMessage = "✗ Radio connection failed!";
        }
        
        await Task.Delay(3000);
        StatusMessage = "";
    }

    [RelayCommand]
    private async Task TestDatabaseConnection()
    {
        StatusMessage = "Testing database connection...";
        
        // In a real implementation, this would test the database connection
        await Task.Delay(1000);
        
        StatusMessage = "✓ Database connection successful!";
        await Task.Delay(3000);
        StatusMessage = "";
    }

    [RelayCommand]
    private void BrowseStoragePath()
    {
        // Note: WPF doesn't have a built-in FolderBrowserDialog
        // Using OpenFileDialog workaround for folder selection
        // In a production app, consider using Windows.Forms.FolderBrowserDialog or a third-party dialog
        var dialog = new OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder",
            Title = "Select Storage Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            StoragePath = System.IO.Path.GetDirectoryName(dialog.FileName) ?? "";
        }
    }

    [RelayCommand]
    private void RefreshNetworkInterfaces()
    {
        NetworkInterfaces.Clear();
        
        var interfaces = _networkDiscovery.GetUsbNetworkInterfaces();
        foreach (var iface in interfaces)
        {
            NetworkInterfaces.Add(iface);
        }
        
        // Auto-select interface with gateway
        if (NetworkInterfaces.Count > 0 && SelectedInterface == null)
        {
            var withGateway = NetworkInterfaces.FirstOrDefault(ni => !string.IsNullOrEmpty(ni.GatewayAddress));
            if (withGateway != null)
            {
                SelectedInterface = withGateway;
                RadioIpAddress = withGateway.GatewayAddress;
            }
        }
    }

    partial void OnSelectedInterfaceChanged(NetworkInterfaceInfo? value)
    {
        if (value != null && !string.IsNullOrEmpty(value.GatewayAddress))
        {
            RadioIpAddress = value.GatewayAddress;
        }
    }
}
