namespace HyteraGateway.UI.Models;

/// <summary>
/// Configuration settings for the UI application
/// </summary>
public class UIConfiguration
{
    // Radio Settings
    public string RadioIpAddress { get; set; } = "192.168.1.1";
    public int RadioControlPort { get; set; } = 50000;
    public int RadioAudioPort { get; set; } = 50001;
    public bool AutoReconnect { get; set; } = true;
    public int DispatcherId { get; set; } = 1;

    // Database Settings
    public string DbHost { get; set; } = "localhost";
    public int DbPort { get; set; } = 3306;
    public string DbName { get; set; } = "easyvol";
    public string DbUsername { get; set; } = "root";
    public string DbPassword { get; set; } = "";

    // API Settings
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
    public int ApiPort { get; set; } = 5000;
    public bool EnableHttps { get; set; } = false;
    public bool EnableSwagger { get; set; } = true;
    public string CorsOrigins { get; set; } = "*";

    // Audio Settings
    public bool RecordingEnabled { get; set; } = true;
    public string StoragePath { get; set; } = "";
    public string AudioFormat { get; set; } = "WAV";
    public int AudioBitrate { get; set; } = 128;

    // FTP Settings
    public bool FtpEnabled { get; set; } = false;
    public string FtpHost { get; set; } = "";
    public int FtpPort { get; set; } = 21;
    public string FtpUsername { get; set; } = "";
    public string FtpPassword { get; set; } = "";
    public string FtpRemotePath { get; set; } = "/recordings";
    public bool FtpAutoUpload { get; set; } = false;
}
