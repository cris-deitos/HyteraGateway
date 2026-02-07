namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for radio connection
/// </summary>
public class RadioConfig
{
    /// <summary>
    /// Connection type: USB (NCM/RNDIS) or Ethernet (direct IP)
    /// </summary>
    public RadioConnectionType ConnectionType { get; set; } = RadioConnectionType.USB;

    /// <summary>
    /// For USB: interface name. For Ethernet: not used
    /// </summary>
    public string InterfaceName { get; set; } = "USB NCM";

    /// <summary>
    /// Radio IP address
    /// - USB: typically 192.168.x.1 (gateway of USB interface)
    /// - Ethernet: any IP on the LAN (e.g., 10.0.0.100)
    /// </summary>
    public string IpAddress { get; set; } = "192.168.1.1";

    /// <summary>
    /// Radio port for control protocol (same for both connection types)
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

/// <summary>
/// Radio connection type enum
/// </summary>
public enum RadioConnectionType
{
    /// <summary>
    /// USB connection via NCM or RNDIS (MD785i, etc.)
    /// Creates point-to-point network interface
    /// </summary>
    USB,
    
    /// <summary>
    /// Direct Ethernet/IP connection (HM785, etc.)
    /// Radio is on the LAN with its own IP
    /// </summary>
    Ethernet,
    
    /// <summary>
    /// Auto-detect: try USB interfaces first, then direct IP
    /// </summary>
    Auto
}
