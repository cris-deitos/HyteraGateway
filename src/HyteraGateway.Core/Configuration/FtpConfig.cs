namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for FTP server (optional)
/// </summary>
public class FtpConfig
{
    /// <summary>
    /// FTP server hostname or IP
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// FTP server port
    /// </summary>
    public int Port { get; set; } = 21;

    /// <summary>
    /// FTP username
    /// </summary>
    public string Username { get; set; } = "anonymous";

    /// <summary>
    /// FTP password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remote directory for audio files
    /// </summary>
    public string RemoteDirectory { get; set; } = "/audio";

    /// <summary>
    /// Enable passive mode
    /// </summary>
    public bool PassiveMode { get; set; } = true;

    /// <summary>
    /// Enable SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; } = false;
}
