namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Main configuration for HyteraGateway application
/// </summary>
public class HyteraGatewayConfig
{
    /// <summary>
    /// Radio connection configuration
    /// </summary>
    public RadioConfig Radio { get; set; } = new();

    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseConfig Database { get; set; } = new();

    /// <summary>
    /// FTP configuration (optional)
    /// </summary>
    public FtpConfig? Ftp { get; set; }

    /// <summary>
    /// Audio processing configuration
    /// </summary>
    public AudioConfig Audio { get; set; } = new();

    /// <summary>
    /// API and WebSocket configuration
    /// </summary>
    public ApiConfig Api { get; set; } = new();
}
