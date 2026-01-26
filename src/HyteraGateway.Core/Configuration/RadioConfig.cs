namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for radio connection
/// </summary>
public class RadioConfig
{
    /// <summary>
    /// USB NCM interface name or identifier
    /// </summary>
    public string InterfaceName { get; set; } = "USB NCM";

    /// <summary>
    /// Radio IP address (typically 192.168.x.x for NCM)
    /// </summary>
    public string IpAddress { get; set; } = "192.168.1.1";

    /// <summary>
    /// Radio port for control protocol
    /// </summary>
    public int ControlPort { get; set; } = 50000;

    /// <summary>
    /// Radio port for audio streaming
    /// </summary>
    public int AudioPort { get; set; } = 50001;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable automatic reconnection
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Reconnection interval in seconds
    /// </summary>
    public int ReconnectIntervalSeconds { get; set; } = 10;
}
