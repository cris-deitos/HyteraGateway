namespace HyteraGateway.UI.Models;

/// <summary>
/// Represents the current status of the HyteraGateway service
/// </summary>
public class ServiceStatus
{
    /// <summary>
    /// Gets or sets whether the service is online and operational
    /// </summary>
    public bool IsOnline { get; set; }
    
    /// <summary>
    /// Gets or sets the number of currently active radios
    /// </summary>
    public int ActiveRadioCount { get; set; }
    
    /// <summary>
    /// Gets or sets the number of calls received today
    /// </summary>
    public int CallsToday { get; set; }
    
    /// <summary>
    /// Gets or sets the total size of recordings in bytes
    /// </summary>
    public long RecordingsSizeBytes { get; set; }
    
    /// <summary>
    /// Gets or sets whether the database connection is active
    /// </summary>
    public bool DatabaseConnected { get; set; }
    
    /// <summary>
    /// Gets or sets whether the FTP connection is active
    /// </summary>
    public bool FtpConnected { get; set; }
}
